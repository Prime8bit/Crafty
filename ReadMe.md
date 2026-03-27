# Crafty Web app
This is meant to be a portfolio piece for demonstrating my knowledge in C#, ASP.NET, and Angular so it is open for issues but closed for pull requests. This repository includes two projects:
- A C#/ASP.NET/Entity Framework backend.
- An Angular frontend.

# Getting the code
This project uses a submodule I created called CraftyCommon which you can see among my repositories. Ensure that submodules are downloaded and current when using this repository.

# How to run
The short version is that you can't without making some minor changes to the code. My application uses Cloudinary as a CDN for user-uploaded media files. Naturally I don't want people to use my code to upload inappropriate media to my cloudinary account so I have replaced my credentials with invalid ones. You can still run the application, but you will need to follow these steps:
- Create a cloudinary account (there are free accounts with limits, but this is what I use for development)
- Create an API key
- If not already there, Navigate to your cloudinary dashboard and open your cloudinary keys page.
- Open API/appsettings.json file and enter in the following data:
    - "CloudName": "The Cloud Name At The Top Left Product Environments Dropdown",
    - "ApiKey": "Your API key",
    - "ApiSecret": "Your API secret"

## Running from docker.
This is the recommended way to run this application in a short amount of time, but I have a disclaimer. For reasons I do not yet understand, when the application is run from docker containers, using firefox to view the client causes all requests to the backend to return 500 server error responses. I have tested this application with chrome and edge and those seem to work just fine. To run this application just use the following from a terminal:

```
docker compose up
```

Once it is up and running you should be able to use the app from chrome or edge using the url `http://localhost:4200`
I recommend you login with the credentials:
Username: zamora
Password: password

## Running the code manually
You will need two terminals to run this code unless you want to run them with background processes. I recommend two sessions so you can see the output of each server separately.

### Dependencies
- .NET SDK 9.0
- Sqlite3
- Node.js
- npm
- The angular cli version 18 or newer (npm install -g @angular/cli)
- It is helpful to add dotnet and npm to your PATH in Windows. In Linux, this is usually already handled for you.
- There are numerous third party libraries used by this repository, but they are all installed using npm for the front-end and nuget for the backend using standard npm and nuget repositories.

### Front-end
- cd client
- npm install
- ng serve
Navigate to http://{ip address}:4200

### Back-end
- dotnet clean
- dotnet build
- cd API
- dotnet run
    - If you get CORS errors between client and server, but postman works, then it is likely that the trusted self-signed certificate expired. This is used for testing https communication.
    - Run "dotnet dev-certs https --trust" then restart your browser. That should temporarily trust the self-signed certificate this app uses.

All accounts use "password" as their password. I recommend you use "zamora" for a regular user and "nate" for a user with an administrator role.

The TestDataTemplate in the server application is used with json-generator.com to generate seed data. It was used during early development, but doesn't work now due to numerous changes to the code. It is in my todo list below to update this. Until then, just use the sqlite database included in this repository.

## Ideas for 2.0+:
- Currently, the update craft page has a huge flaw - it is possible to delete images then "cancel" the update. 
    - This should be prevented as it will cause the images to fail to load and because they have been deleted already, trying to delete them again will fail.
    - I also need to do something when a user uploads new media, but then cancels the creation/updating of crafts. This will leak storage space on my CDN.
- I need to handle the use case where a seller updates crafts between the time a user adds items to their cart and the time they checkout.
- I would like to use MVVM for the front-end. I don't know if this is wise and I am certain that google recommends against OOP in angular for greater efficiency. This is something I will have to research.
- Add logging and reporting.
- Convert the craftSort option into an enum in wishlistscomponent and craftlistscomponent.
- Implement separate billing and shipping addresses.
- Create a curated list of categories for crafts and allow users to assign categories to their crafts. This has the potential to be abused if a person puts all categories on their products so I would need to somehow prevent that.
- Create the ability to leave craft reviews. This would be a good example of when using a nosql database would be helpful.
- Create a CDN abstraction layer so switching to a different CDN can be as painless as possible. 
- Sqlite doesn't store UTC dates well. I might want to fix that manually. This probably insn't improtant for a portfolio piece. If I were to do this project again I wouldn't use sqlite at all. It was a recommendation from the angular/asp.net course I took that in hindsight was a bad idea. Migrating to another database engine may involve numerous refactors to my code and will definitely require a custom migration script. 
- My client currently passes the raw password when registering for a new account. This is a security flaw. I should consider using a better solution.
- Update seed data json. Now that the app has the ability to create all entities from the front-end, there isn't much need for seed-data.
- Deploy my app using mariadb instead of sqlite. 
    - I chose not to do this because I want people to be able to run my application quickly. If I used mariadb, then I would need to create a custom mariadb container with my database, or create a script that imports a database export on startup if the tables don't exist. Using sqlite I can just bundle my database with the code and you can run.
    - Obfuscate my javascript code during deployment so it is harder to reverse engineer in production.
    - Figure out why running in docker containers causes 500 server errors from all request from Firefox to the backend container on the same machine. Chrome/edge are fine.

# Common Questions
## Why this app
I wanted to create something that was familiar but not as commonly asked in interviews as "Create youtube" so I chose "Create Etsy." Unfortunately, part way through implementing this I realized that "Create Etsy" is no diffent than "Create an e-commerce store", but I was already in too deep so I stuck with it.

## Why not host this yourself for easier viewing
I am not a cybersecurity expert. I don't want to have to spend the time that is necessary to prevent problems like a bot using my frontend to automate creating crafts with inappropriate pictures, videos or 3d models. I want to ensure that when I run my app on my laptop, that my sample data is the only thing