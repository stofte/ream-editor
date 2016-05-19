# we dont want to fail right away
npm run int-test
$code = $LASTEXITCODE
type $env:ELECTRON_OUT\linq-editor.log
exit $code
