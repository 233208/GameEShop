#!/bin/sh
set -e

# Czekaj na bazê danych, a¿ bêdzie gotowa
until dotnet ef database update --project GameEShop/GameEShop.csproj; do
  echo "Czekam na bazê danych..."
  sleep 5
done

# Uruchom aplikacjê
exec "$@"
