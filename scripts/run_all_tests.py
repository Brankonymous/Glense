#!/usr/bin/env python3
"""
Run orchestration for chat tests:
- ensure docker compose is up (if available)
- ensure AccountService and ChatService are running (starts them if needed)
- run the PS test scripts located in services/Glense.ChatService/tests/

This script aims to work on Windows (PowerShell available). It starts dotnet services
in background redirecting logs to tmp/*.txt and waits for /health endpoints.

Usage:
  python run_all_tests.py           # bring up containers (if compose), start services if needed, run PS scripts + python tests
  python run_all_tests.py --python-only  # only run python tests against already-running services
"""
from __future__ import annotations
import argparse
import json
import os
import subprocess
import sys
import time
from pathlib import Path
from typing import Optional, Tuple
from urllib.request import Request, urlopen
from urllib.error import HTTPError, URLError

ROOT = Path(__file__).resolve().parent.parent
CHAT_PROJECT = ROOT / 'services' / 'Glense.ChatService' / 'Glense.ChatService.csproj'
ACCOUNT_PROJECT = ROOT / 'services' / 'Glense.AccountService' / 'Glense.AccountService.csproj'
CHAT_HEALTH = os.environ.get('CHAT_HEALTH', 'http://localhost:5002/health')
ACCOUNT_HEALTH = os.environ.get('ACCOUNT_HEALTH', 'http://localhost:5000/health')
TMP = ROOT / 'tmp'
TMP.mkdir(exist_ok=True)


def load_dotenv_file(dotenv_path: Path):
	if not dotenv_path.exists():
		return
	for line in dotenv_path.read_text().splitlines():
		line = line.strip()
		if not line or line.startswith('#'):
			continue
		if '=' not in line:
			continue
		k, v = line.split('=', 1)
		os.environ.setdefault(k.strip(), v.strip())


def which(cmd: str) -> Optional[str]:
	"""Cross-platform check for command existence (simple shim)."""
	from shutil import which as _which

	return _which(cmd)


def try_docker_compose_up() -> None:
	# prefer 'docker compose' (v2) then fall back to 'docker-compose'
	compose_variants = [['docker', 'compose', 'up', '-d', '--build'], ['docker-compose', 'up', '-d', '--build']]
	if not (ROOT / 'docker-compose.yml').exists():
		print('No docker-compose.yml found at repo root; skipping docker-compose step')
		return

	for cmd in compose_variants:
		if which(cmd[0]) is None:
			continue
		try:
			print('Running:', ' '.join(cmd))
			res = subprocess.run(cmd, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, timeout=300)
			print(res.stdout)
			if res.returncode == 0:
				print('docker compose up done')
				return
			else:
				print('docker compose returned non-zero:', res.returncode)
		except subprocess.TimeoutExpired:
			print('docker compose timed out')
			return
	print('docker compose not available or failed; continuing without containers')


def wait_for(url: str, timeout: int = 60) -> Tuple[bool, str]:
	deadline = time.time() + timeout
	last_err = None
	req = Request(url, headers={'User-Agent': 'run_all_tests.py'})
	while time.time() < deadline:
		try:
			with urlopen(req, timeout=5) as r:
				data = r.read().decode('utf-8')
				return True, data
		except Exception as e:
			last_err = e
			time.sleep(1)
	return False, str(last_err)


def start_dotnet_project(proj_path: Path, logfile: Path):
	if not proj_path.exists():
		print('Project file not found:', proj_path)
		return None
	cmd = ['dotnet', 'run', '--project', str(proj_path)]
	print('Starting:', ' '.join(cmd))
	f = open(logfile, 'w', encoding='utf-8')
	# Start in background
	p = subprocess.Popen(cmd, stdout=f, stderr=subprocess.STDOUT)
	return p


def run_powershell_script(script_path: Path) -> int:
	# Use PowerShell Core or Windows PowerShell if available
	pw = which('pwsh') or which('powershell') or 'powershell'
	cmd = [pw, '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', str(script_path)]
	print('Running script:', ' '.join(cmd))
	res = subprocess.run(cmd, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
	print(res.stdout)
	return res.returncode


# ---- HTTP helper & python tests ----


def _http_request(method: str, url: str, data=None, headers=None, timeout: int = 10):
	headers = headers or {}
	data_bytes = None
	if data is not None:
		data_bytes = json.dumps(data).encode('utf-8')
		headers.setdefault('Content-Type', 'application/json')
	req = Request(url, data=data_bytes, headers=headers, method=method)
	try:
		with urlopen(req, timeout=timeout) as r:
			resp = r.read().decode('utf-8')
			ct = r.headers.get('Content-Type', '')
			if 'application/json' in ct:
				return r.getcode(), json.loads(resp)
			return r.getcode(), resp
	except HTTPError as he:
		try:
			body = he.read().decode('utf-8')
			return he.code, json.loads(body) if body and body.strip().startswith('{') else body
		except Exception:
			return he.code, str(he)
	except URLError as ue:
		return None, str(ue)
	except Exception as e:
		return None, str(e)


def pretty_print(title: str, obj) -> None:
	print('\n' + '=' * 60)
	print(title)
	print('-' * 60)
	if obj is None:
		print('(no response)')
	elif isinstance(obj, (dict, list)):
		print(json.dumps(obj, indent=2, ensure_ascii=False))
	else:
		try:
			parsed = json.loads(obj)
			print(json.dumps(parsed, indent=2, ensure_ascii=False))
		except Exception:
			print(obj)
	print('=' * 60 + '\n')


def register_user(base: str, username: str, email: str, password: str):
	url = f"{base}/api/auth/register"
	payload = {
		"Username": username,
		"Email": email,
		"Password": password,
		"ConfirmPassword": password,
	}
	return _http_request('POST', url, data=payload)


def login_user(base: str, username_or_email: str, password: str):
	url = f"{base}/api/auth/login"
	payload = {"UsernameOrEmail": username_or_email, "Password": password}
	code, body = _http_request('POST', url, data=payload)
	if code == 200 and isinstance(body, dict) and ('token' in body or 'Token' in body):
		# accept 'token' or 'Token' keys
		token = body.get('token') or body.get('Token')
		return token, body
	return None, body


def create_chat(base: str, token: str, topic: str):
	url = f"{base}/api/chats"
	headers = {'Authorization': f'Bearer {token}'}
	return _http_request('POST', url, data={'Topic': topic}, headers=headers)


def post_message(base: str, token: str, chat_id: str, sender: str, content: str):
	url = f"{base}/api/chats/{chat_id}/messages"
	headers = {'Authorization': f'Bearer {token}'}
	payload = {'Sender': sender, 'Content': content}
	return _http_request('POST', url, data=payload, headers=headers)


def get_messages(base: str, token: str, chat_id: str, cursor: Optional[str] = None, page_size: int = 50):
	url = f"{base}/api/chats/{chat_id}/messages?pageSize={page_size}"
	if cursor:
		url += f"&cursor={cursor}"
	headers = {'Authorization': f'Bearer {token}'}
	return _http_request('GET', url, headers=headers)


def python_tests() -> int:
	account_base = os.environ.get('ACCOUNT_URLS', 'http://localhost:5000')
	chat_base = os.environ.get('CHAT_URLS', 'http://localhost:5002')
	pretty_print('Test runner: endpoints', {'account': account_base, 'chat': chat_base})

	pw = 'TestPass!123'
	users = [
		('user_a', 'user_a@example.test'),
		('user_b', 'user_b@example.test'),
		('user_c', 'user_c@example.test'),
	]
	tokens = {}

	# register/login users
	for uname, email in users:
		code, body = register_user(account_base, uname, email, pw)
		pretty_print(f'Register {uname} (code={code})', body)
		if code != 200:
			pretty_print(f'Register {uname} failed; attempting login', body)
		token, resp = login_user(account_base, uname, pw)
		if token:
			pretty_print(f'Login {uname} succeeded', {'token_preview': token[:32] + '...'})
			tokens[uname] = token
		else:
			pretty_print(f'Login {uname} failed', resp)

	# create chats
	chat_ids = []
	if tokens.get('user_a'):
		code, body = create_chat(chat_base, tokens['user_a'], 'A-B Chat')
		pretty_print('Create chat A-B', {'code': code, 'body': body})
		if isinstance(body, dict) and ('id' in body or 'Id' in body):
			chat_ids.append(body.get('id') or body.get('Id'))

	if tokens.get('user_a'):
		code, body = create_chat(chat_base, tokens['user_a'], 'Group ABC')
		pretty_print('Create chat Group ABC', {'code': code, 'body': body})
		if isinstance(body, dict) and ('id' in body or 'Id' in body):
			chat_ids.append(body.get('id') or body.get('Id'))

	# send messages and test pagination
	if chat_ids:
		chat = chat_ids[0]
		# send 5 messages
		for i in range(5):
			sender = 'user' if (i % 2 == 0) else 'system'
			who = 'user_a' if i % 2 == 0 else 'user_b'
			token = tokens.get(who) or list(tokens.values())[0]
			content = f'Message #{i+1} from {who}'
			code, body = post_message(chat_base, token, chat, sender, content)
			pretty_print(f'Post message {i+1} to chat {chat} (code={code})', body)

		# fetch with small page size to exercise cursor pagination
		all_msgs = []
		cursor = None
		while True:
			code, body = get_messages(chat_base, tokens[next(iter(tokens))], chat, cursor=cursor, page_size=2)
			pretty_print(f'Get messages (cursor={cursor})', {'code': code, 'body': body})
			if not isinstance(body, dict):
				break
			items = body.get('items') or []
			all_msgs.extend(items)
			cursor = body.get('nextCursor')
			if not cursor:
				break

		pretty_print('Aggregated messages', {'count': len(all_msgs), 'messages': all_msgs})

	# list chats
	if tokens.get('user_a'):
		code, body = _http_request('GET', f"{chat_base}/api/chats?pageSize=10", headers={'Authorization': f'Bearer {tokens['user_a']}'} )
		pretty_print('List chats', {'code': code, 'body': body})

	return 0


def main(argv=None) -> int:
	parser = argparse.ArgumentParser()
	parser.add_argument('--python-only', action='store_true', help='Run only the Python tests against already-running services (skip docker/ps scripts)')
	args = parser.parse_args(argv)

	load_dotenv_file(ROOT / '.env')

	# Step 1: ensure docker containers (if any) are up
	if not args.python_only:
		try_docker_compose_up()

	# Step 2: ensure services running (start via dotnet run if not healthy)
	procs = []

	ok, _ = wait_for(ACCOUNT_HEALTH, timeout=3)
	if not ok:
		print('AccountService not healthy; starting via dotnet run...')
		p = start_dotnet_project(ACCOUNT_PROJECT, TMP / 'account_out.txt')
		if p:
			procs.append(p)
		ok, data = wait_for(ACCOUNT_HEALTH, timeout=30)
		if not ok:
			print('AccountService failed to become healthy:', data)
	else:
		print('AccountService healthy')

	ok, _ = wait_for(CHAT_HEALTH, timeout=3)
	if not ok:
		print('ChatService not healthy; starting via dotnet run...')
		p = start_dotnet_project(CHAT_PROJECT, TMP / 'chat_out.txt')
		if p:
			procs.append(p)
		ok, data = wait_for(CHAT_HEALTH, timeout=30)
		if not ok:
			print('ChatService failed to become healthy:', data)
	else:
		print('ChatService healthy')

	# Step 3: optionally run PowerShell test scripts
	if not args.python_only:
		tests_dir = ROOT / 'services' / 'Glense.ChatService' / 'tests'
		scripts = ['test_chat.ps1', 'send_two_messages.ps1']
		for s in scripts:
			path = tests_dir / s
			if path.exists():
				rc = run_powershell_script(path)
				if rc != 0:
					print(f'Script {s} exited with code {rc}')
			else:
				print('Test script not found:', path)

	# Step 4: run Python tests
	try:
		ret = python_tests()
	except Exception as e:
		print('Python tests failed with exception:', e)
		ret = 2

	print('All done. Logs:')
	for f in TMP.glob('*.txt'):
		print('-', f)

	# Note: do not forcibly kill background dotnet processes here; leave them for inspection.
	return ret


if __name__ == '__main__':
	sys.exit(main())

