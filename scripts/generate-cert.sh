#!/bin/bash

# Load environment variables
source .env

# Create certificates directory if it doesn't exist
mkdir -p ./certs

# Generate development certificate
dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p $CERT_PASSWORD

# Trust the certificate
dotnet dev-certs https --trust

echo "SSL certificate generated successfully!"
echo "Location: ./certs/aspnetapp.pfx"
echo "Password: $CERT_PASSWORD" 