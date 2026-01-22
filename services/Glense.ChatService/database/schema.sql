-- chats table
CREATE TABLE IF NOT EXISTS chats (
  id uuid PRIMARY KEY,
  topic varchar(200) NOT NULL,
  created_at_utc timestamptz NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_chats_created_at ON chats (created_at_utc);

-- messages table
CREATE TABLE IF NOT EXISTS messages (
  id uuid PRIMARY KEY,
  chat_id uuid NOT NULL REFERENCES chats(id) ON DELETE CASCADE,
  sender smallint NOT NULL,
  content text NOT NULL,
  created_at_utc timestamptz NOT NULL
);
CREATE INDEX IF NOT EXISTS IX_messages_chat_id_created_at ON messages (chat_id, created_at_utc);
