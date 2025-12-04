# Mikroservisi koji treba da se urade

1. Account
2. Video katalozi / streaming
3. Donations
4. Live chat

### Account
Profili, mogu medjusobno da se prate, sistem notifikacije, login/registracija

### Video katalozi / live streaming

Upload videa, start live streaming-a, mogucnost kreiranja plejlisti. Subscription deo gde se vide klipovi subscribe-ovanih profila

### Donacije

Donacije izmedju naloga i donacije kreatorima sajta (potencijalno neka porukica uz donaciju?)

### Live chat

u toku live streaminga profili mogu da se dopisuju sa strimerom ali i drugima. Strimer moze da ima neke permisije da blokira naloge itd

# Instrukcije za instalaciju

`Visual Studio 2022 / Visual studio code` je preporuka

Instalirati dodatne stvari: 
- `.NET8`
- `nodejs v22`

# Instrukcije za rad

1. Setupujte `git pp` da moze da se prati cije je grana, mislim da ce ispasti lepse kad prezentujemo
1. Koristimo github tasks za pracenje sprintova i kreiranje taskova
2. Pravimo PR koji mora da ima bar jedan approval pre merge-a


## Šema baze

Mozda necemo koristiti. Koliko vidim po kursu treba svako da ima svoju bazu?

![Glense Database Schema](schema-Glense.svg)

# Kako pokrenuti projekat
1. Preko konzole lociraj se na `Glense.Server/` folder
2. **dotnet run**

# Pre-commit Hook

When you first clone the repository, run the setup script to install the pre-commit hook:

```bash
# Make sure you're in the repository root
./scripts/setup-hooks.sh
```

# Git PP

Using `git pp` you can automatically add your username as a prefix when pushing the current branch. 
Example:
If your branch is called `fix-bugs` and you setup username to John using `./setup-pp.sh John` - pushed branched should look like this: `John/fix-bugs`

```bash
# Make sure you're in the repository root
./scripts/setup-pp.sh username
```


This project includes a pre-commit hook that automatically formats C# code before each commit.

The hook will automatically run and format your C# code. If any files are modified by formatting, the commit will be blocked and you'll need to stage the formatted files and commit again.

You can also run formatting manually:

```bash
# Place yourself on some directory:
dotnet format Glense.sln
```
