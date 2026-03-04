 Wonga Authentication System

A secure, containerized authentication system built with Angular, .NET 8 Web API, and **PostgreSQL**. This project demonstrates a complete user lifecycle: Registration, Authentication (JWT), and Protected Resource Access.

I have written a script (.bat file called run watch wonga) to help start the angular app up all you have to do is edit the File and give it the exact path of the project and it will run ng serve and run the start up the app for you. e.g this is its path on my local (C:\Projects\Wonga\wonga.client).

 Start (Docker)

Ensure you have {Docker Desktop} installed. To spin up the entire environment (API + Database), run the following command from the root directory of the project:
bash or use gitbash or cmd doesn't matter 
docker-compose up --build

and just Like that the App will be "cruising nicely without fear of contradiction".
