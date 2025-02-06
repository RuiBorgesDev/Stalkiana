<pre>
  _________  __           .__    __    .__                         
 /   _____/_/  |_ _____   |  |  |  | __|__|_____     ____  _____   
 \_____  \ \   __\\__  \  |  |  |  |/ /|  |\__  \   /    \ \__  \  
 /        \ |  |   / __ \_|  |__|    < |  | / __ \_|   |  \ / __ \_
/_______  / |__|  (____  /|____/|__|_ \|__|(____  /|___|  /(____  /
        \/             \/            \/         \/      \/      \/ 
</pre>

A simple command-line tool for stalking an instagram account.

**Note**: Please be aware that using this tool is against Instagram's terms of service. Use it responsibly and at your own risk.

## Features

- Get a list of followings and followers from a user;
- Save and log if the user's following/follower count has changed;
- Check who stopped/started following the user;
- Check who the user stopped/started following;
- Download profile pictures from a given public or private user;
- Track multiple users at once with logs saved in different directories

## Prerequisites

- .NET SDK 8.0 installed on your machine;
- Instagram credentials (cookies);

## Installation

1. Clone this repository to your local machine:

   ```shell
   git clone https://github.com/RuiPedroDev/Stalkiana
   ```

2. Navigate to the project directory:

    ```shell
    cd Stalkiana
    ```

3. Run the tool:

    ```shell
    dotnet run [username]
    ```
    [username] (optional): The Instagram username to track. If not provided, the program will prompt you to enter the username during execution.

## Information and Usage

This tool has two main functions:

1- Downloading a profile picture (this works in both public and private instagram accounts).

2- Tracking followers/followings by keeping a local list of the followers/followings of an instagram user and then upon execution check if the follower/following count has changed compared to the last execution time, if it changed it determines the difference by comparing the new list with the old list, when the tool finishes execution, it logs the results into a results.txt file.

This tool requires the instagram cookie in order to work.
If you don't know how to get the cookie, here are the steps to get it:


1. Launch your browser and open the inspect element panel (CTRL + SHIFT + C or F12).


2. Go to Instagram and Log in.


3. Once Logged in, go to the network tab and type "graphql" or "query" on the filter at the top and click on any of the requests that appear bellow.


4. Go to request headers and search for the cookie, triple click the full value and copy it.