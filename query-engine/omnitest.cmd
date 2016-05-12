REM curl -vX GET  http://localhost:2000/checkreadystatus
REM curl -vX GET  http://localhost:2000/projects
REM curl -vX POST http://localhost:2000/updatebuffer -d @updatebuffer.json
curl -vX POST http://localhost:2000/autocomplete -d @autocomplete.json
