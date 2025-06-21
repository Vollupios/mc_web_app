#!/bin/bash

# Script to fix common markdown linting issues

# Function to fix markdown files
fix_markdown() {
    local file="$1"
    echo "Fixing $file..."
    
    # Create backup
    cp "$file" "$file.bak"
    
    # Fix common patterns using sed
    sed -i '
        # Add blank line before headings if missing
        /^##/ {
            x
            /^$/ !{
                x
                i\

                b
            }
            x
        }
        
        # Add blank line after headings if missing
        /^##/ {
            N
            /\n[^#]/ {
                s/\n/\n\n/
            }
        }
        
        # Add blank lines around lists
        /^- / {
            x
            /^$/ !{
                x
                i\

                b
            }
            x
        }
        
        # Add blank lines around fenced code blocks
        /^```/ {
            x
            /^$/ !{
                x
                i\

                b
            }
            x
        }
        
    ' "$file"
}

# Fix all markdown files
for file in *.md; do
    if [ -f "$file" ]; then
        fix_markdown "$file"
    fi
done

echo "Markdown files fixed!"
