#!/bin/sh
start_webapp()
{
    echo "Running WebApp..."
    cd "WebApp"
    echo "$(pwd)"
    powershell -Command "dotnet run"
}

start_bot()
{
    echo "Running Bot..."
    cd "Bot"
    powershell -Command "dotnet run"
}

open_web()
{
    sleep 2
    echo "opening website"
    powershell -Command "Start 'http://localhost:5047'"
}

echo "Starting Bot..."

REPO_URL="https://github.com/RedEagleP1/P1-CSharp-Bot.git"
DIR_NAME="Bot-Location"

# Github info
read -p "Enter your GitHub username: " GITHUB_USERNAME
read -sp "Enter your GitHub personal access token: " GITHUB_PAT

# Github URL
AUTH_REPO_URL="https://${GITHUB_USERNAME}:${GITHUB_PAT}@github.com/RedEagleP1/P1-CSharp-Bot.git"

# Check if the directory exists
if [ -d "$DIR_NAME" ]; then
    echo "Directory $DIR_NAME already exists. Updating repository..."

    # Enter Directory
    cd "$DIR_NAME" || { echo "Failed to enter directory $DIR_NAME"; exit 1; }
    
    # Fetch and pull
    git fetch
    git pull

    # Update the database
    cd "Discord Bot"
    echo "Updating the database..."
    powershell -Command "dotnet ef database update --project 'Models'"

    # Run Bot, WebApp, Webpage
    start_bot&
    start_webapp&
    open_web&
    wait
else
    echo "Directory $DIR_NAME does not exist. Cloning repository..."
    git clone "$AUTH_REPO_URL" "$DIR_NAME"
    cd "$DIR_NAME" || { echo "Failed to enter directory $DIR_NAME"; exit 1; }

    # Install Scoop and dependencies
    echo "Installing dependencies..."
    powershell -Command "
    Set-ExecutionPolicy RemoteSigned -scope CurrentUser -Force;
    Invoke-Expression (New-Object System.Net.WebClient).DownloadString('https://get.scoop.sh');
    scoop install podman;
    podman machine init;
    podman machine start;
    scoop bucket add versions;
    scoop install versions/dotnet6-sdk;
    dotnet tool install --global dotnet-ef;
    "

    echo "Bot added. Run the script again to start the bot. ***!!!DO NOT RUN UNTIL ALL STEPS IN THE READ ME ARE DONE!!!***"
fi