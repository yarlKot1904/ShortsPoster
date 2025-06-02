# ShortsPoster
setup\
&emsp;    build\
&emsp;&emsp;        docker compose run app dotnet ef database update\
&emsp;    migration\
&emsp;&emsp;        docker compose run --rm migrator\
run\
&emsp;    docker compose up -d app\
tests\
&emsp; docker build -f Dockerfile.tests -t shortsposter-tests .\
&emsp; docker run --rm shortsposter-tests

