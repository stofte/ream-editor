const Application = require('spectron').Application
const assert = require('assert')
const path = require('path');

const appPath = path.normalize(`${__dirname}/linq-editor-win32-x64/linq-editor.exe`);

describe('application launch', function () {
    this.timeout(10000);

    beforeEach(function () {
        this.app = new Application({
            path: appPath
        });
        return this.app.start();
    });

    afterEach(function () {
        if (this.app && this.app.isRunning()) {
          return this.app.stop();
        }
    });

    it('shows an initial window', function () {
        return this.app.client.getWindowCount().then(function (count) {
            assert.equal(count, 1)
        });
    });
})
