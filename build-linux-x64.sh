#!/bin/bash
RUNTIME_ID="linux-x64"
OUTPUT_DIR="./publish_output"

echo "--- Starting Stalkiana .NET Publish Script ---"
mkdir -p "$OUTPUT_DIR"

echo "Publishing project for Runtime ID: $RUNTIME_ID to $OUTPUT_DIR..."

dotnet publish -c Release -r "$RUNTIME_ID" --self-contained true -p:PublishSingleFile=true --output "$OUTPUT_DIR"

EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "--- Publish Successful ---"

    ABS_OUTPUT_DIR=$(realpath "$OUTPUT_DIR")

    if [ -d "$ABS_OUTPUT_DIR" ]; then
        echo "Adding $ABS_OUTPUT_DIR to PATH for the current session."

        export PATH="$ABS_OUTPUT_DIR:$PATH"

        echo "PATH updated temporarily."
        echo "Current PATH: $PATH"
        echo ""
        echo "To make this change permanent, add the following line to your shell configuration file"
        echo "(e.g., ~/.bashrc or ~/.zshrc):"
        echo "  export PATH=\"$ABS_OUTPUT_DIR:\$PATH\""
        echo "Then, run 'source ~/.bashrc' (or the relevant file) or open a new terminal."

    else
        echo "Error: Could not find the absolute path for the output directory: $OUTPUT_DIR"
        exit 1
    fi
else
    echo "--- Publish Failed ---"
    echo "Error: 'dotnet publish' command failed with exit code $EXIT_CODE."
    exit $EXIT_CODE
fi

echo "--- Script Finished ---"