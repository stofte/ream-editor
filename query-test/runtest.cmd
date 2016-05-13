REM curl -vX GET  http://localhost:8111/checkreadystatus
REM curl -vX POST http://localhost:8111/executequery -d @test\query1.json
REM curl -vX POST http://localhost:8111/executequery -d @test\query2.json
REM curl -vX POST http://localhost:8111/executequery -d @test\query3.json
REM curl -vX POST http://localhost:8111/executequery -d @test\query3.json
curl -vX POST http://localhost:8111/querytemplate -d @test\query1.json  --header "Content-Type: application/json"
REM curl -vX GET http://localhost:8111/stopserver
