# Instrukcije za instalaciju

`Visual Studio 2022`
`.NET8` i `nodejs v22`

# Instrukcije za rad

1. Koristimo [Linear](https://linear.app/glense/team/GLE/active) za tracking. Pogledajte [backlog](https://linear.app/glense/team/GLE/backlog) npr. setupujte github s linear-om vrv. olaksava posao.
2. Kada hocete da napravite granu, <b> prvo napravite issue </b> na Linear-u. 
![alt text](image.png)
Kopirajte ime grane i ukucajte u terminalu: <br>
`git checkout -b ime_grane`
3. Kada napravite commit-ove itd. preporuka je da to rebase-ujete u jedan commit. [Link do tutoriala](https://stackoverflow.com/questions/5189560/how-do-i-squash-my-last-n-commits-together)
4. Kada zavrsite sa radom na feature-u <b>napravite PR</b>. To mozete odraditi sa github sajta a moze i preko terminala. Push na master je zabranjen i treba vam _jedan approve_ za merge u master.

# Kako pokrenuti bazu

1. Instalirati SQL server (mssql) ekstenziju
2. Napraviti konekciju s bazom
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
4. U browser otici na https://localhost:<port>/api/test/test-connection

Dopunite ovaj readme s jos korisnih informacije ako ih nadjete.