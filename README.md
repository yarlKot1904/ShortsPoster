# ShortsPoster
setup
    build
        docker compose run app dotnet ef database update
    migration
        docker compose run --rm migrator
run
    docker compose up -d app
tests
    
