# we dont want to fail right away
npm run int-test
$code = $LASTEXITCODE
if ($code -isnot 0) {
    # only if we failed
    type $env:ELECTRON_OUT\linq-editor.log    
}
exit $code
