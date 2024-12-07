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

#Kako pokrenuti projekat

1. Pokrenuti sql server
2. U vs konzoli ukucati dotnet run
3. Pogledati na kom je portu u konzoli koja iskoci
4. U browser otici na https://localhost:<port>/api/test/test-connection

# Ostalo
Dopunite ovaj readme s jos korisnih informacije ako ih nadjete.