# Обновление базы данных
Write-Host "Applying EF Core migrations..."
dotnet ef database update

# Запуск ngrok (в фоновом процессе)
Write-Host "Starting ngrok tunnel..."
Start-Process ngrok "http --domain=ample-informally-crawdad.ngrok-free.app 5166"

# Запуск ASP.NET Core приложения
Write-Host "Running ASP.NET Core app..."
dotnet run
