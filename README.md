# Mikroservisi koji treba da se urade

1. Account
2. Video katalozi / streaming
3. Donations
4. Live chat
5. Recommendation Engine

### Account
Profili, mogu medjusobno da se prate, sistem notifikacije, login/registracija

### Video katalozi / live streaming

Upload videa, start live streaming-a, mogucnost kreiranja plejlisti. Subscription deo gde se vide klipovi subscribe-ovanih profila

### Donacije

Donacije izmedju naloga i donacije kreatorima sajta (potencijalno neka porukica uz donaciju?)

### Live chat

u toku live streaminga profili mogu da se dopisuju sa strimerom ali i drugima. Strimer moze da ima neke permisije da blokira naloge itd

### Recommendation Engine
AI recommendation koji bi pravio analitiku i iz te analitike bi za svakog usera posebno imao personalizovani main feed

# Instrukcije za instalaciju

`Visual Studio 2022`
`.NET8` i `nodejs v22`

# Instrukcije za rad

1. Koristimo [Linear](https://linear.app/glense/team/GLE/active) za tracking. Pogledajte [backlog](https://linear.app/glense/team/GLE/backlog) npr. setupujte github s linear-om vrv. olaksava posao.
2. Kada hocete da napravite granu, <b> prvo napravite issue </b> na Linear-u. 
![alt text](image.png)
Kopirajte ime grane i ukucajte u terminalu: <br>
`git checkout -b ime_grane`
3. Kada zavrsite sa radom na feature-u <b>napravite PR</b>. To mozete odraditi sa github sajta a moze i preko terminala. Push na master granu je zabranjen sam po sebi, tj. treba vam _jedan approve_ za merge u master.

# Kako pokrenuti bazu

1. Instalirati SQL server (mssql) ekstenziju
2. Pokrenuti `glense.sql` (mozes preko ekstenzije)
3. Napraviti konekciju s bazom
    - `ctrl+alt+D` -> Add connection -> Podesi parametre
    - Profile name: **Glense**
    - Server name: **TVOJE_IME\SQLEXPRESS** *(TODO: Istraziti sta je ovo)*
    - Trust server certificate - **yes**
    - Authentication type: **Windows authentication** *(TODO: Istraziti sta je ovo)*
    - Database name: **Glense**

*TODO: Napraviti automatizaciju i videti kako da se ovaj proces generalizuje, posto trenutno ne kontamo nista*

## Šema baze

![Glense Database Schema](schema-Glense.svg)

# Kako pokrenuti projekat
1. Preko konzole lociraj se na `Glense.Server/` folder
2. **dotnet run**

# Ostalo

(Markove instrukcije - deprecated)
1. Pokrenuti sql server
2. U vs konzoli ukucati dotnet run
3. Pogledati na kom je portu u konzoli koja iskoci
4. U browser otici na `https://localhost:<port>/api/test/test-connection`

Dopunite ovaj readme s jos korisnih informacije ako ih nadjete.