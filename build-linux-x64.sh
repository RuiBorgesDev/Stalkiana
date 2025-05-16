#!/bin/bash
runtimeID="linux-x64"
publishConfiguration="Release"

CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${CYAN}--- Starting Stalkiana .NET Publish Script (Linux x64) ---${NC}"

stalkianaBasePath="$HOME/.stalkiana"

if [ ! -d "$stalkianaBasePath" ]; then
    echo -e "${CYAN}Creating output directory: $stalkianaBasePath${NC}"
    if mkdir -p "$stalkianaBasePath"; then
        echo -e "${GREEN}Successfully created output directory: $stalkianaBasePath${NC}"
    else
        echo -e "${RED}Failed to create output directory '$stalkianaBasePath'.${NC}" >&2
        exit 1
    fi
else
    echo -e "${CYAN}Output directory already exists: $stalkianaBasePath${NC}"
fi

echo -e "${CYAN}Publishing project/solution for Runtime ID: $runtimeID to $stalkianaBasePath...${NC}"

projectPath="$PWD"

dotnetArgs=(
    "publish"
    "$projectPath"
    "-c" "$publishConfiguration"
    "-r" "$runtimeID"
    "--self-contained" "true"
    "-p:PublishSingleFile=true"
    "-p:PublishDir=$stalkianaBasePath"
)

echo -e "${CYAN}Running: dotnet ${dotnetArgs[*]}${NC}"

if dotnet "${dotnetArgs[@]}"; then
    exitCode=$?
else
    exitCode=$?
    echo -e "${RED}Failed to start 'dotnet publish'. Ensure the .NET SDK is installed and in your PATH.${NC}" >&2
fi

if [ $exitCode -eq 0 ]; then
    echo -e "${GREEN}--- Publish Successful ---${NC}\n"

    absoluteOutputDir="$stalkianaBasePath"

    echo -e "${GREEN}The application was published to: $absoluteOutputDir${NC}"
    echo -e "\n${YELLOW}Attempting to add '$absoluteOutputDir' to the User PATH in .bashrc...${NC}"

    bashrc_file="$HOME/.bashrc"
    path_entry_line="export PATH=\"$absoluteOutputDir:\$PATH\""
    path_entry_comment="# Added by Stalkiana publish script"

    if [ -f "$bashrc_file" ]; then
        if grep -qF "$path_entry_line" "$bashrc_file"; then
            echo -e "${CYAN}'$absoluteOutputDir' path entry seems to already be present in $bashrc_file. No changes made.${NC}"
        elif grep -qF "$absoluteOutputDir" "$bashrc_file"; then
             echo -e "${CYAN}'$absoluteOutputDir' seems to be in $bashrc_file in a different form. No changes made. Please verify manually.${NC}"
        else
            echo -e "\n$path_entry_comment" >> "$bashrc_file"
            echo -e "$path_entry_line" >> "$bashrc_file"
            echo -e "${GREEN}Successfully added '$absoluteOutputDir' to $bashrc_file.${NC}"
            echo -e "${YELLOW}IMPORTANT: You MUST source your $bashrc_file (e.g., run 'source $bashrc_file') or open a new terminal for this change to take effect.${NC}"
            echo -e "${YELLOW}For the current session, you can update the PATH by running:${NC}"
            echo -e "${YELLOW}  export PATH=\"$absoluteOutputDir:\$PATH\"${NC}"
        fi
    else
        echo -e "${RED}Could not find $bashrc_file. Cannot automatically add to PATH.${NC}" >&2
        echo -e "${YELLOW}Please add '$absoluteOutputDir' to your PATH manually by adding the following line to your shell configuration file (e.g., .bashrc, .zshrc):${NC}"
        echo -e "${YELLOW}  $path_entry_line${NC}"
    fi
else
    echo -e "${RED}--- Publish Failed ---${NC}" >&2
    echo -e "${RED}'dotnet publish' command failed with exit code $exitCode.${NC}" >&2
    exit $exitCode
fi

echo -e "\n${CYAN}--- Script Finished ---${NC}"