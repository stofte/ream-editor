'use strict';

const electron = require('electron');
const ipc = electron.ipcMain;
const app = electron.app;  // Module to control application life.
const BrowserWindow = electron.BrowserWindow;  // Module to create native browser window.
const Menu = electron.Menu;
const MenuItem = electron.MenuItem;

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow = null;
let cleanedUpQueryEngine = false;
let cleanedUpOmnisharp = false;

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
app.on('ready', function() {
    // Create the browser window.
    mainWindow = new BrowserWindow({width: 1100, height: 900, title: 'Frank'});
    
    var template = [{
            label: 'File',
            submenu: [{
                    label: 'Connections',
                    accelerator: 'ctrl+d',
                    click: () => 
                        mainWindow.webContents.send('application-event', 'connections-panel'),
                }, {
                    label: 'Exit',
                    click: () => 
                        mainWindow.close(),
                },
            ],
        }, {
            label: 'Debug',
            submenu: [{
                    label: 'Reload',
                    accelerator: 'ctrl+r',
                    click: () => 
                        mainWindow.webContents.executeJavaScript("location.reload();"),
                },{
                    label: 'DevTools',
                    accelerator: 'ctrl+shift-i',
                    click: () => 
                        mainWindow.webContents.openDevTools()
                },
            ],
        },
    ];

    mainWindow.setMenu(Menu.buildFromTemplate(template));
    //mainWindow.maximize();

    // and load the index.html of the app.
    mainWindow.loadURL('file://' + __dirname.replace(/\\/g,'/') + '/index.html');

    // Open the DevTools.
    mainWindow.webContents.openDevTools();

    // msg app that we're closing and wait for the response
    mainWindow.on('close', function(event) {
        if (!(cleanedUpQueryEngine && cleanedUpOmnisharp)) {
            mainWindow.webContents.send('application-event', 'close');
            mainWindow.hide();
            event.preventDefault();
            return false;
        }
    });

    // Emitted when the window is closed.
    mainWindow.on('closed', function() {
        // Dereference the window object, usually you would store windows
        // in an array if your app supports multi windows, this is the time
        // when you should delete the corresponding element.
        mainWindow = null;
    });

    ipc.on('application-event', function(event, msg) {
        if (msg === 'close-query-engine') {
            cleanedUpQueryEngine = true;
        } else if (msg === 'close-omnisharp') {
            cleanedUpOmnisharp = true;
        }
        if (cleanedUpQueryEngine && cleanedUpOmnisharp) {
            mainWindow.close();
        }
    });
});

// Quit when all windows are closed.
app.on('window-all-closed', function() {
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if (process.platform != 'darwin') {
        app.quit();
    }
});