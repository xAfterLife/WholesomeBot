# WholesomeBot - A VRChat DiscordBot 
## DiscordBot that interacts with the VRChat API to search Users- Host Instances etc.

The Bots functionality is going to increase from time to time as need is seen.
Currently working on Hosting Instances on Discord Server as public, private (with user pings) or with a required Role.
You can search User by either DisplayName or ID and get back a well formated view of the JSON-Data
Verification of VRChat user and linking them to a Discord Account is in the works as well.

## What is being used 

* .Net 6.0 
* Discord.Net 
* Microsoft.Extensions.DependencyInjection
* VRChat.API 
* (Probably EF.CORE in the Future)


## How to install and run

1. clone this project
2. set up environment variables for:
   * token = Discordbot Token
   * vrc_username = VRChat username
   * vrc_password = VRChat password
3. Compile & Run the project


## Find a bug?
If you found an issue or would like to submit an improvement to this project, please submit an issue using the issue tab above. 
If you would like to submit a PR with a fix, refrence the issue you created
