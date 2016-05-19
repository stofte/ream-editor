try { 
    npm run int-test
    exit 0
}
catch {
    type $env:ELECTRON_OUT\linq-editor.log
    exit 1
}