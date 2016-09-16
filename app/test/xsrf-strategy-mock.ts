import { XSRFStrategy, Request } from '@angular/http';
// avoids trying to reading the undefined cookie collection
// (not sure why it's undefined, electron should provide the dom ...)
// https://github.com/angular/angular/issues/9294
const XSRFStrategyMock = { provide: XSRFStrategy, useValue: {
    configureRequest: function() { /* */ }
} };
export default XSRFStrategyMock;
