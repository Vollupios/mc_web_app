#!/bin/bash

# Fix Markdown Linting Issues Script
# Fixes MD040 (missing language specifiers), MD022 (heading spacing), MD032 (list spacing), etc.

echo "🔧 Fixing Markdown linting issues..."

# Function to fix MD040 - Add language specifiers to code blocks
fix_md040() {
    local file="$1"
    echo "Fixing MD040 in $file..."
    
    # Add 'text' language to generic code blocks
    sed -i 's/^```$/```text/' "$file"
    
    # Common patterns - fix specific cases
    sed -i 's/^```bash$/```bash/' "$file"
    sed -i 's/^```powershell$/```powershell/' "$file"
    sed -i 's/^```sql$/```sql/' "$file"
    sed -i 's/^```csharp$/```csharp/' "$file"
    sed -i 's/^```json$/```json/' "$file"
    sed -i 's/^```xml$/```xml/' "$file"
}

# Function to fix MD022 - Add blank lines around headings
fix_md022() {
    local file="$1"
    echo "Fixing MD022 in $file..."
    
    # Add blank line before headings that don't have one
    sed -i '/^[^#]$/,/^###\? / { /^###\? /i\
    }' "$file"
}

# Function to fix MD032 - Add blank lines around lists
fix_md032() {
    local file="$1"
    echo "Fixing MD032 in $file..."
    
    # Add blank line before lists that don't have one
    sed -i '/^[^-*+]$/,/^[-*+] / { /^[-*+] /i\
    }' "$file"
    
    # Add blank line after lists that don't have one
    sed -i '/^[-*+] /,/^[^-*+]/ { /^[^-*+]/i\
    }' "$file"
}

# Function to fix MD031 - Add blank lines around fenced code blocks
fix_md031() {
    local file="$1"
    echo "Fixing MD031 in $file..."
    
    # Add blank line before code blocks
    sed -i '/^[^`]$/,/^```/ { /^```/i\
    }' "$file"
    
    # Add blank line after code blocks
    sed -i '/^```$/,/^[^`]/ { /^[^`]/i\
    }' "$file"
}

# Function to fix MD009 - Remove trailing spaces
fix_md009() {
    local file="$1"
    echo "Fixing MD009 in $file..."
    sed -i 's/[[:space:]]*$//' "$file"
}

# Function to fix MD036 - Convert emphasis to headings where appropriate
fix_md036() {
    local file="$1"
    echo "Fixing MD036 in $file..."
    
    # Convert **text** at start of line to ### text
    sed -i 's/^\*\*\([^*]*\)\*\*$/### \1/' "$file"
}

# Get all markdown files
files=$(find /home/pcjv/IntranetDocumentos -name "*.md" -type f | grep -v node_modules | grep -v .git)

for file in $files; do
    echo "📝 Processing: $file"
    
    # Create backup
    cp "$file" "$file.bak"
    
    # Apply fixes
    fix_md009 "$file"
    fix_md040 "$file"
    fix_md031 "$file"
    fix_md032 "$file"
    fix_md022 "$file"
    fix_md036 "$file"
    
    echo "✅ Fixed: $file"
done

echo "🎉 All markdown files processed!"
echo "💾 Backups created with .bak extension"
