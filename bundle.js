/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};

/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {

/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId])
/******/ 			return installedModules[moduleId].exports;

/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			exports: {},
/******/ 			id: moduleId,
/******/ 			loaded: false
/******/ 		};

/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);

/******/ 		// Flag the module as loaded
/******/ 		module.loaded = true;

/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}


/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;

/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;

/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";

/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(0);
/******/ })
/************************************************************************/
/******/ ([
/* 0 */
/***/ function(module, exports, __webpack_require__) {

	/* WEBPACK VAR INJECTION */(function(global) {"use strict";
	__webpack_require__(1);
	var event_target_1 = __webpack_require__(2);
	var define_property_1 = __webpack_require__(4);
	var register_element_1 = __webpack_require__(5);
	var property_descriptor_1 = __webpack_require__(6);
	var timers_1 = __webpack_require__(8);
	var utils_1 = __webpack_require__(3);
	var set = 'set';
	var clear = 'clear';
	var blockingMethods = ['alert', 'prompt', 'confirm'];
	var _global = typeof window == 'undefined' ? global : window;
	timers_1.patchTimer(_global, set, clear, 'Timeout');
	timers_1.patchTimer(_global, set, clear, 'Interval');
	timers_1.patchTimer(_global, set, clear, 'Immediate');
	timers_1.patchTimer(_global, 'request', 'cancelMacroTask', 'AnimationFrame');
	timers_1.patchTimer(_global, 'mozRequest', 'mozCancel', 'AnimationFrame');
	timers_1.patchTimer(_global, 'webkitRequest', 'webkitCancel', 'AnimationFrame');
	for (var i = 0; i < blockingMethods.length; i++) {
	    var name = blockingMethods[i];
	    utils_1.patchMethod(_global, name, function (delegate, symbol, name) {
	        return function (s, args) {
	            return Zone.current.run(delegate, _global, args, name);
	        };
	    });
	}
	event_target_1.eventTargetPatch(_global);
	property_descriptor_1.propertyDescriptorPatch(_global);
	utils_1.patchClass('MutationObserver');
	utils_1.patchClass('WebKitMutationObserver');
	utils_1.patchClass('FileReader');
	define_property_1.propertyPatch();
	register_element_1.registerElementPatch(_global);
	// Treat XMLHTTPRequest as a macrotask.
	patchXHR(_global);
	var XHR_TASK = utils_1.zoneSymbol('xhrTask');
	function patchXHR(window) {
	    function findPendingTask(target) {
	        var pendingTask = target[XHR_TASK];
	        return pendingTask;
	    }
	    function scheduleTask(task) {
	        var data = task.data;
	        data.target.addEventListener('readystatechange', function () {
	            if (data.target.readyState === XMLHttpRequest.DONE) {
	                if (!data.aborted) {
	                    task.invoke();
	                }
	            }
	        });
	        var storedTask = data.target[XHR_TASK];
	        if (!storedTask) {
	            data.target[XHR_TASK] = task;
	        }
	        setNative.apply(data.target, data.args);
	        return task;
	    }
	    function placeholderCallback() {
	    }
	    function clearTask(task) {
	        var data = task.data;
	        // Note - ideally, we would call data.target.removeEventListener here, but it's too late
	        // to prevent it from firing. So instead, we store info for the event listener.
	        data.aborted = true;
	        return clearNative.apply(data.target, data.args);
	    }
	    var setNative = utils_1.patchMethod(window.XMLHttpRequest.prototype, 'send', function () { return function (self, args) {
	        var zone = Zone.current;
	        var options = {
	            target: self,
	            isPeriodic: false,
	            delay: null,
	            args: args,
	            aborted: false
	        };
	        return zone.scheduleMacroTask('XMLHttpRequest.send', placeholderCallback, options, scheduleTask, clearTask);
	    }; });
	    var clearNative = utils_1.patchMethod(window.XMLHttpRequest.prototype, 'abort', function (delegate) { return function (self, args) {
	        var task = findPendingTask(self);
	        if (task && typeof task.type == 'string') {
	            // If the XHR has already completed, do nothing.
	            if (task.cancelFn == null) {
	                return;
	            }
	            task.zone.cancelTask(task);
	        }
	        // Otherwise, we are trying to abort an XHR which has not yet been sent, so there is no task to cancel. Do nothing.
	    }; });
	}
	/// GEO_LOCATION
	if (_global['navigator'] && _global['navigator'].geolocation) {
	    utils_1.patchPrototype(_global['navigator'].geolocation, [
	        'getCurrentPosition',
	        'watchPosition'
	    ]);
	}

	/* WEBPACK VAR INJECTION */}.call(exports, (function() { return this; }())))

/***/ },
/* 1 */
/***/ function(module, exports) {

	/* WEBPACK VAR INJECTION */(function(global) {;
	;
	var Zone = (function (global) {
	    var Zone = (function () {
	        function Zone(parent, zoneSpec) {
	            this._properties = null;
	            this._parent = parent;
	            this._name = zoneSpec ? zoneSpec.name || 'unnamed' : '<root>';
	            this._properties = zoneSpec && zoneSpec.properties || {};
	            this._zoneDelegate = new ZoneDelegate(this, this._parent && this._parent._zoneDelegate, zoneSpec);
	        }
	        Object.defineProperty(Zone, "current", {
	            get: function () { return _currentZone; },
	            enumerable: true,
	            configurable: true
	        });
	        ;
	        Object.defineProperty(Zone, "currentTask", {
	            get: function () { return _currentTask; },
	            enumerable: true,
	            configurable: true
	        });
	        ;
	        Object.defineProperty(Zone.prototype, "parent", {
	            get: function () { return this._parent; },
	            enumerable: true,
	            configurable: true
	        });
	        ;
	        Object.defineProperty(Zone.prototype, "name", {
	            get: function () { return this._name; },
	            enumerable: true,
	            configurable: true
	        });
	        ;
	        Zone.prototype.get = function (key) {
	            var current = this;
	            while (current) {
	                if (current._properties.hasOwnProperty(key)) {
	                    return current._properties[key];
	                }
	                current = current._parent;
	            }
	        };
	        Zone.prototype.fork = function (zoneSpec) {
	            if (!zoneSpec)
	                throw new Error('ZoneSpec required!');
	            return this._zoneDelegate.fork(this, zoneSpec);
	        };
	        Zone.prototype.wrap = function (callback, source) {
	            if (typeof callback !== 'function') {
	                throw new Error('Expecting function got: ' + callback);
	            }
	            var _callback = this._zoneDelegate.intercept(this, callback, source);
	            var zone = this;
	            return function () {
	                return zone.runGuarded(_callback, this, arguments, source);
	            };
	        };
	        Zone.prototype.run = function (callback, applyThis, applyArgs, source) {
	            if (applyThis === void 0) { applyThis = null; }
	            if (applyArgs === void 0) { applyArgs = null; }
	            if (source === void 0) { source = null; }
	            var oldZone = _currentZone;
	            _currentZone = this;
	            try {
	                return this._zoneDelegate.invoke(this, callback, applyThis, applyArgs, source);
	            }
	            finally {
	                _currentZone = oldZone;
	            }
	        };
	        Zone.prototype.runGuarded = function (callback, applyThis, applyArgs, source) {
	            if (applyThis === void 0) { applyThis = null; }
	            if (applyArgs === void 0) { applyArgs = null; }
	            if (source === void 0) { source = null; }
	            var oldZone = _currentZone;
	            _currentZone = this;
	            try {
	                try {
	                    return this._zoneDelegate.invoke(this, callback, applyThis, applyArgs, source);
	                }
	                catch (error) {
	                    if (this._zoneDelegate.handleError(this, error)) {
	                        throw error;
	                    }
	                }
	            }
	            finally {
	                _currentZone = oldZone;
	            }
	        };
	        Zone.prototype.runTask = function (task, applyThis, applyArgs) {
	            task.runCount++;
	            if (task.zone != this)
	                throw new Error('A task can only be run in the zone which created it! (Creation: ' +
	                    task.zone.name + '; Execution: ' + this.name + ')');
	            var previousTask = _currentTask;
	            _currentTask = task;
	            var oldZone = _currentZone;
	            _currentZone = this;
	            try {
	                if (task.type == 'macroTask' && task.data && !task.data.isPeriodic) {
	                    task.cancelFn = null;
	                }
	                try {
	                    return this._zoneDelegate.invokeTask(this, task, applyThis, applyArgs);
	                }
	                catch (error) {
	                    if (this._zoneDelegate.handleError(this, error)) {
	                        throw error;
	                    }
	                }
	            }
	            finally {
	                _currentZone = oldZone;
	                _currentTask = previousTask;
	            }
	        };
	        Zone.prototype.scheduleMicroTask = function (source, callback, data, customSchedule) {
	            return this._zoneDelegate.scheduleTask(this, new ZoneTask('microTask', this, source, callback, data, customSchedule, null));
	        };
	        Zone.prototype.scheduleMacroTask = function (source, callback, data, customSchedule, customCancel) {
	            return this._zoneDelegate.scheduleTask(this, new ZoneTask('macroTask', this, source, callback, data, customSchedule, customCancel));
	        };
	        Zone.prototype.scheduleEventTask = function (source, callback, data, customSchedule, customCancel) {
	            return this._zoneDelegate.scheduleTask(this, new ZoneTask('eventTask', this, source, callback, data, customSchedule, customCancel));
	        };
	        Zone.prototype.cancelTask = function (task) {
	            var value = this._zoneDelegate.cancelTask(this, task);
	            task.runCount = -1;
	            task.cancelFn = null;
	            return value;
	        };
	        Zone.__symbol__ = __symbol__;
	        return Zone;
	    }());
	    ;
	    var ZoneDelegate = (function () {
	        function ZoneDelegate(zone, parentDelegate, zoneSpec) {
	            this._taskCounts = { microTask: 0, macroTask: 0, eventTask: 0 };
	            this.zone = zone;
	            this._parentDelegate = parentDelegate;
	            this._forkZS = zoneSpec && (zoneSpec && zoneSpec.onFork ? zoneSpec : parentDelegate._forkZS);
	            this._forkDlgt = zoneSpec && (zoneSpec.onFork ? parentDelegate : parentDelegate._forkDlgt);
	            this._interceptZS = zoneSpec && (zoneSpec.onIntercept ? zoneSpec : parentDelegate._interceptZS);
	            this._interceptDlgt = zoneSpec && (zoneSpec.onIntercept ? parentDelegate : parentDelegate._interceptDlgt);
	            this._invokeZS = zoneSpec && (zoneSpec.onInvoke ? zoneSpec : parentDelegate._invokeZS);
	            this._invokeDlgt = zoneSpec && (zoneSpec.onInvoke ? parentDelegate : parentDelegate._invokeDlgt);
	            this._handleErrorZS = zoneSpec && (zoneSpec.onHandleError ? zoneSpec : parentDelegate._handleErrorZS);
	            this._handleErrorDlgt = zoneSpec && (zoneSpec.onHandleError ? parentDelegate : parentDelegate._handleErrorDlgt);
	            this._scheduleTaskZS = zoneSpec && (zoneSpec.onScheduleTask ? zoneSpec : parentDelegate._scheduleTaskZS);
	            this._scheduleTaskDlgt = zoneSpec && (zoneSpec.onScheduleTask ? parentDelegate : parentDelegate._scheduleTaskDlgt);
	            this._invokeTaskZS = zoneSpec && (zoneSpec.onInvokeTask ? zoneSpec : parentDelegate._invokeTaskZS);
	            this._invokeTaskDlgt = zoneSpec && (zoneSpec.onInvokeTask ? parentDelegate : parentDelegate._invokeTaskDlgt);
	            this._cancelTaskZS = zoneSpec && (zoneSpec.onCancelTask ? zoneSpec : parentDelegate._cancelTaskZS);
	            this._cancelTaskDlgt = zoneSpec && (zoneSpec.onCancelTask ? parentDelegate : parentDelegate._cancelTaskDlgt);
	            this._hasTaskZS = zoneSpec && (zoneSpec.onHasTask ? zoneSpec : parentDelegate._hasTaskZS);
	            this._hasTaskDlgt = zoneSpec && (zoneSpec.onHasTask ? parentDelegate : parentDelegate._hasTaskDlgt);
	        }
	        ZoneDelegate.prototype.fork = function (targetZone, zoneSpec) {
	            return this._forkZS
	                ? this._forkZS.onFork(this._forkDlgt, this.zone, targetZone, zoneSpec)
	                : new Zone(targetZone, zoneSpec);
	        };
	        ZoneDelegate.prototype.intercept = function (targetZone, callback, source) {
	            return this._interceptZS
	                ? this._interceptZS.onIntercept(this._interceptDlgt, this.zone, targetZone, callback, source)
	                : callback;
	        };
	        ZoneDelegate.prototype.invoke = function (targetZone, callback, applyThis, applyArgs, source) {
	            return this._invokeZS
	                ? this._invokeZS.onInvoke(this._invokeDlgt, this.zone, targetZone, callback, applyThis, applyArgs, source)
	                : callback.apply(applyThis, applyArgs);
	        };
	        ZoneDelegate.prototype.handleError = function (targetZone, error) {
	            return this._handleErrorZS
	                ? this._handleErrorZS.onHandleError(this._handleErrorDlgt, this.zone, targetZone, error)
	                : true;
	        };
	        ZoneDelegate.prototype.scheduleTask = function (targetZone, task) {
	            try {
	                if (this._scheduleTaskZS) {
	                    return this._scheduleTaskZS.onScheduleTask(this._scheduleTaskDlgt, this.zone, targetZone, task);
	                }
	                else if (task.scheduleFn) {
	                    task.scheduleFn(task);
	                }
	                else if (task.type == 'microTask') {
	                    scheduleMicroTask(task);
	                }
	                else {
	                    throw new Error('Task is missing scheduleFn.');
	                }
	                return task;
	            }
	            finally {
	                if (targetZone == this.zone) {
	                    this._updateTaskCount(task.type, 1);
	                }
	            }
	        };
	        ZoneDelegate.prototype.invokeTask = function (targetZone, task, applyThis, applyArgs) {
	            try {
	                return this._invokeTaskZS
	                    ? this._invokeTaskZS.onInvokeTask(this._invokeTaskDlgt, this.zone, targetZone, task, applyThis, applyArgs)
	                    : task.callback.apply(applyThis, applyArgs);
	            }
	            finally {
	                if (targetZone == this.zone && (task.type != 'eventTask') && !(task.data && task.data.isPeriodic)) {
	                    this._updateTaskCount(task.type, -1);
	                }
	            }
	        };
	        ZoneDelegate.prototype.cancelTask = function (targetZone, task) {
	            var value;
	            if (this._cancelTaskZS) {
	                value = this._cancelTaskZS.onCancelTask(this._cancelTaskDlgt, this.zone, targetZone, task);
	            }
	            else if (!task.cancelFn) {
	                throw new Error('Task does not support cancellation, or is already canceled.');
	            }
	            else {
	                value = task.cancelFn(task);
	            }
	            if (targetZone == this.zone) {
	                // this should not be in the finally block, because exceptions assume not canceled.
	                this._updateTaskCount(task.type, -1);
	            }
	            return value;
	        };
	        ZoneDelegate.prototype.hasTask = function (targetZone, isEmpty) {
	            return this._hasTaskZS && this._hasTaskZS.onHasTask(this._hasTaskDlgt, this.zone, targetZone, isEmpty);
	        };
	        ZoneDelegate.prototype._updateTaskCount = function (type, count) {
	            var counts = this._taskCounts;
	            var prev = counts[type];
	            var next = counts[type] = prev + count;
	            if (next < 0) {
	                throw new Error('More tasks executed then were scheduled.');
	            }
	            if (prev == 0 || next == 0) {
	                var isEmpty = {
	                    microTask: counts.microTask > 0,
	                    macroTask: counts.macroTask > 0,
	                    eventTask: counts.eventTask > 0,
	                    change: type
	                };
	                try {
	                    this.hasTask(this.zone, isEmpty);
	                }
	                finally {
	                    if (this._parentDelegate) {
	                        this._parentDelegate._updateTaskCount(type, count);
	                    }
	                }
	            }
	        };
	        return ZoneDelegate;
	    }());
	    var ZoneTask = (function () {
	        function ZoneTask(type, zone, source, callback, options, scheduleFn, cancelFn) {
	            this.runCount = 0;
	            this.type = type;
	            this.zone = zone;
	            this.source = source;
	            this.data = options;
	            this.scheduleFn = scheduleFn;
	            this.cancelFn = cancelFn;
	            this.callback = callback;
	            var self = this;
	            this.invoke = function () {
	                try {
	                    return zone.runTask(self, this, arguments);
	                }
	                finally {
	                    drainMicroTaskQueue();
	                }
	            };
	        }
	        return ZoneTask;
	    }());
	    function __symbol__(name) { return '__zone_symbol__' + name; }
	    ;
	    var symbolSetTimeout = __symbol__('setTimeout');
	    var symbolPromise = __symbol__('Promise');
	    var symbolThen = __symbol__('then');
	    var _currentZone = new Zone(null, null);
	    var _currentTask = null;
	    var _microTaskQueue = [];
	    var _isDrainingMicrotaskQueue = false;
	    var _uncaughtPromiseErrors = [];
	    var _drainScheduled = false;
	    function scheduleQueueDrain() {
	        if (!_drainScheduled && !_currentTask && _microTaskQueue.length == 0) {
	            // We are not running in Task, so we need to kickstart the microtask queue.
	            if (global[symbolPromise]) {
	                global[symbolPromise].resolve(0)[symbolThen](drainMicroTaskQueue);
	            }
	            else {
	                global[symbolSetTimeout](drainMicroTaskQueue, 0);
	            }
	        }
	    }
	    function scheduleMicroTask(task) {
	        scheduleQueueDrain();
	        _microTaskQueue.push(task);
	    }
	    function consoleError(e) {
	        var rejection = e && e.rejection;
	        if (rejection) {
	            console.error('Unhandled Promise rejection:', rejection instanceof Error ? rejection.message : rejection, '; Zone:', e.zone.name, '; Task:', e.task && e.task.source, '; Value:', rejection);
	        }
	        console.error(e);
	    }
	    function drainMicroTaskQueue() {
	        if (!_isDrainingMicrotaskQueue) {
	            _isDrainingMicrotaskQueue = true;
	            while (_microTaskQueue.length) {
	                var queue = _microTaskQueue;
	                _microTaskQueue = [];
	                for (var i = 0; i < queue.length; i++) {
	                    var task = queue[i];
	                    try {
	                        task.zone.runTask(task, null, null);
	                    }
	                    catch (e) {
	                        consoleError(e);
	                    }
	                }
	            }
	            while (_uncaughtPromiseErrors.length) {
	                var uncaughtPromiseErrors = _uncaughtPromiseErrors;
	                _uncaughtPromiseErrors = [];
	                var _loop_1 = function(i) {
	                    var uncaughtPromiseError = uncaughtPromiseErrors[i];
	                    try {
	                        uncaughtPromiseError.zone.runGuarded(function () { throw uncaughtPromiseError; });
	                    }
	                    catch (e) {
	                        consoleError(e);
	                    }
	                };
	                for (var i = 0; i < uncaughtPromiseErrors.length; i++) {
	                    _loop_1(i);
	                }
	            }
	            _isDrainingMicrotaskQueue = false;
	            _drainScheduled = false;
	        }
	    }
	    function isThenable(value) {
	        return value && value.then;
	    }
	    function forwardResolution(value) { return value; }
	    function forwardRejection(rejection) { return ZoneAwarePromise.reject(rejection); }
	    var symbolState = __symbol__('state');
	    var symbolValue = __symbol__('value');
	    var source = 'Promise.then';
	    var UNRESOLVED = null;
	    var RESOLVED = true;
	    var REJECTED = false;
	    var REJECTED_NO_CATCH = 0;
	    function makeResolver(promise, state) {
	        return function (v) {
	            resolvePromise(promise, state, v);
	            // Do not return value or you will break the Promise spec.
	        };
	    }
	    function resolvePromise(promise, state, value) {
	        if (promise[symbolState] === UNRESOLVED) {
	            if (value instanceof ZoneAwarePromise && value[symbolState] !== UNRESOLVED) {
	                clearRejectedNoCatch(value);
	                resolvePromise(promise, value[symbolState], value[symbolValue]);
	            }
	            else if (isThenable(value)) {
	                value.then(makeResolver(promise, state), makeResolver(promise, false));
	            }
	            else {
	                promise[symbolState] = state;
	                var queue = promise[symbolValue];
	                promise[symbolValue] = value;
	                for (var i = 0; i < queue.length;) {
	                    scheduleResolveOrReject(promise, queue[i++], queue[i++], queue[i++], queue[i++]);
	                }
	                if (queue.length == 0 && state == REJECTED) {
	                    promise[symbolState] = REJECTED_NO_CATCH;
	                    try {
	                        throw new Error("Uncaught (in promise): " + value);
	                    }
	                    catch (e) {
	                        var error = e;
	                        error.rejection = value;
	                        error.promise = promise;
	                        error.zone = Zone.current;
	                        error.task = Zone.currentTask;
	                        _uncaughtPromiseErrors.push(error);
	                        scheduleQueueDrain();
	                    }
	                }
	            }
	        }
	        // Resolving an already resolved promise is a noop.
	        return promise;
	    }
	    function clearRejectedNoCatch(promise) {
	        if (promise[symbolState] === REJECTED_NO_CATCH) {
	            promise[symbolState] = REJECTED;
	            for (var i = 0; i < _uncaughtPromiseErrors.length; i++) {
	                if (promise === _uncaughtPromiseErrors[i].promise) {
	                    _uncaughtPromiseErrors.splice(i, 1);
	                    break;
	                }
	            }
	        }
	    }
	    function scheduleResolveOrReject(promise, zone, chainPromise, onFulfilled, onRejected) {
	        clearRejectedNoCatch(promise);
	        var delegate = promise[symbolState] ? onFulfilled || forwardResolution : onRejected || forwardRejection;
	        zone.scheduleMicroTask(source, function () {
	            try {
	                resolvePromise(chainPromise, true, zone.run(delegate, null, [promise[symbolValue]]));
	            }
	            catch (error) {
	                resolvePromise(chainPromise, false, error);
	            }
	        });
	    }
	    var ZoneAwarePromise = (function () {
	        function ZoneAwarePromise(executor) {
	            var promise = this;
	            promise[symbolState] = UNRESOLVED;
	            promise[symbolValue] = []; // queue;
	            try {
	                executor && executor(makeResolver(promise, RESOLVED), makeResolver(promise, REJECTED));
	            }
	            catch (e) {
	                resolvePromise(promise, false, e);
	            }
	        }
	        ZoneAwarePromise.resolve = function (value) {
	            return resolvePromise(new this(null), RESOLVED, value);
	        };
	        ZoneAwarePromise.reject = function (error) {
	            return resolvePromise(new this(null), REJECTED, error);
	        };
	        ZoneAwarePromise.race = function (values) {
	            var resolve;
	            var reject;
	            var promise = new this(function (res, rej) { resolve = res; reject = rej; });
	            function onResolve(value) { promise && (promise = null || resolve(value)); }
	            function onReject(error) { promise && (promise = null || reject(error)); }
	            for (var _i = 0, values_1 = values; _i < values_1.length; _i++) {
	                var value = values_1[_i];
	                if (!isThenable(value)) {
	                    value = this.resolve(value);
	                }
	                value.then(onResolve, onReject);
	            }
	            return promise;
	        };
	        ZoneAwarePromise.all = function (values) {
	            var resolve;
	            var reject;
	            var promise = new this(function (res, rej) { resolve = res; reject = rej; });
	            var count = 0;
	            var resolvedValues = [];
	            function onReject(error) { promise && reject(error); promise = null; }
	            for (var _i = 0, values_2 = values; _i < values_2.length; _i++) {
	                var value = values_2[_i];
	                if (!isThenable(value)) {
	                    value = this.resolve(value);
	                }
	                value.then((function (index) { return function (value) {
	                    resolvedValues[index] = value;
	                    count--;
	                    if (promise && !count) {
	                        resolve(resolvedValues);
	                    }
	                    promise == null;
	                }; })(count), onReject);
	                count++;
	            }
	            if (!count)
	                resolve(resolvedValues);
	            return promise;
	        };
	        ZoneAwarePromise.prototype.then = function (onFulfilled, onRejected) {
	            var chainPromise = new ZoneAwarePromise(null);
	            var zone = Zone.current;
	            if (this[symbolState] == UNRESOLVED) {
	                this[symbolValue].push(zone, chainPromise, onFulfilled, onRejected);
	            }
	            else {
	                scheduleResolveOrReject(this, zone, chainPromise, onFulfilled, onRejected);
	            }
	            return chainPromise;
	        };
	        ZoneAwarePromise.prototype.catch = function (onRejected) {
	            return this.then(null, onRejected);
	        };
	        return ZoneAwarePromise;
	    }());
	    var NativePromise = global[__symbol__('Promise')] = global.Promise;
	    global.Promise = ZoneAwarePromise;
	    if (NativePromise) {
	        var NativePromiseProtototype = NativePromise.prototype;
	        var NativePromiseThen_1 = NativePromiseProtototype[__symbol__('then')]
	            = NativePromiseProtototype.then;
	        NativePromiseProtototype.then = function (onResolve, onReject) {
	            var nativePromise = this;
	            return new ZoneAwarePromise(function (resolve, reject) {
	                NativePromiseThen_1.call(nativePromise, resolve, reject);
	            }).then(onResolve, onReject);
	        };
	    }
	    return global.Zone = Zone;
	})(typeof window === 'undefined' ? global : window);

	/* WEBPACK VAR INJECTION */}.call(exports, (function() { return this; }())))

/***/ },
/* 2 */
/***/ function(module, exports, __webpack_require__) {

	"use strict";
	var utils_1 = __webpack_require__(3);
	var WTF_ISSUE_555 = 'Anchor,Area,Audio,BR,Base,BaseFont,Body,Button,Canvas,Content,DList,Directory,Div,Embed,FieldSet,Font,Form,Frame,FrameSet,HR,Head,Heading,Html,IFrame,Image,Input,Keygen,LI,Label,Legend,Link,Map,Marquee,Media,Menu,Meta,Meter,Mod,OList,Object,OptGroup,Option,Output,Paragraph,Pre,Progress,Quote,Script,Select,Source,Span,Style,TableCaption,TableCell,TableCol,Table,TableRow,TableSection,TextArea,Title,Track,UList,Unknown,Video';
	var NO_EVENT_TARGET = 'ApplicationCache,EventSource,FileReader,InputMethodContext,MediaController,MessagePort,Node,Performance,SVGElementInstance,SharedWorker,TextTrack,TextTrackCue,TextTrackList,WebKitNamedFlow,Worker,WorkerGlobalScope,XMLHttpRequest,XMLHttpRequestEventTarget,XMLHttpRequestUpload,IDBRequest,IDBOpenDBRequest,IDBDatabase,IDBTransaction,IDBCursor,DBIndex'.split(',');
	var EVENT_TARGET = 'EventTarget';
	function eventTargetPatch(_global) {
	    var apis = [];
	    var isWtf = _global['wtf'];
	    if (isWtf) {
	        // Workaround for: https://github.com/google/tracing-framework/issues/555
	        apis = WTF_ISSUE_555.split(',').map(function (v) { return 'HTML' + v + 'Element'; }).concat(NO_EVENT_TARGET);
	    }
	    else if (_global[EVENT_TARGET]) {
	        apis.push(EVENT_TARGET);
	    }
	    else {
	        // Note: EventTarget is not available in all browsers,
	        // if it's not available, we instead patch the APIs in the IDL that inherit from EventTarget
	        apis = NO_EVENT_TARGET;
	    }
	    for (var i = 0; i < apis.length; i++) {
	        var type = _global[apis[i]];
	        utils_1.patchEventTargetMethods(type && type.prototype);
	    }
	}
	exports.eventTargetPatch = eventTargetPatch;


/***/ },
/* 3 */
/***/ function(module, exports) {

	/* WEBPACK VAR INJECTION */(function(global) {/**
	 * Suppress closure compiler errors about unknown 'process' variable
	 * @fileoverview
	 * @suppress {undefinedVars}
	 */
	"use strict";
	exports.zoneSymbol = Zone['__symbol__'];
	var _global = typeof window == 'undefined' ? global : window;
	function bindArguments(args, source) {
	    for (var i = args.length - 1; i >= 0; i--) {
	        if (typeof args[i] === 'function') {
	            args[i] = Zone.current.wrap(args[i], source + '_' + i);
	        }
	    }
	    return args;
	}
	exports.bindArguments = bindArguments;
	;
	function patchPrototype(prototype, fnNames) {
	    var source = prototype.constructor['name'];
	    var _loop_1 = function(i) {
	        var name_1 = fnNames[i];
	        var delegate = prototype[name_1];
	        if (delegate) {
	            prototype[name_1] = (function (delegate) {
	                return function () {
	                    return delegate.apply(this, bindArguments(arguments, source + '.' + name_1));
	                };
	            })(delegate);
	        }
	    };
	    for (var i = 0; i < fnNames.length; i++) {
	        _loop_1(i);
	    }
	}
	exports.patchPrototype = patchPrototype;
	;
	exports.isWebWorker = (typeof WorkerGlobalScope !== 'undefined' && self instanceof WorkerGlobalScope);
	exports.isNode = (typeof process !== 'undefined' && {}.toString.call(process) === '[object process]');
	exports.isBrowser = !exports.isNode && !exports.isWebWorker && !!(typeof window !== 'undefined' && window['HTMLElement']);
	function patchProperty(obj, prop) {
	    var desc = Object.getOwnPropertyDescriptor(obj, prop) || {
	        enumerable: true,
	        configurable: true
	    };
	    // A property descriptor cannot have getter/setter and be writable
	    // deleting the writable and value properties avoids this error:
	    //
	    // TypeError: property descriptors must not specify a value or be writable when a
	    // getter or setter has been specified
	    delete desc.writable;
	    delete desc.value;
	    // substr(2) cuz 'onclick' -> 'click', etc
	    var eventName = prop.substr(2);
	    var _prop = '_' + prop;
	    desc.set = function (fn) {
	        if (this[_prop]) {
	            this.removeEventListener(eventName, this[_prop]);
	        }
	        if (typeof fn === 'function') {
	            var wrapFn = function (event) {
	                var result;
	                result = fn.apply(this, arguments);
	                if (result != undefined && !result)
	                    event.preventDefault();
	            };
	            this[_prop] = wrapFn;
	            this.addEventListener(eventName, wrapFn, false);
	        }
	        else {
	            this[_prop] = null;
	        }
	    };
	    desc.get = function () {
	        return this[_prop];
	    };
	    Object.defineProperty(obj, prop, desc);
	}
	exports.patchProperty = patchProperty;
	;
	function patchOnProperties(obj, properties) {
	    var onProperties = [];
	    for (var prop in obj) {
	        if (prop.substr(0, 2) == 'on') {
	            onProperties.push(prop);
	        }
	    }
	    for (var j = 0; j < onProperties.length; j++) {
	        patchProperty(obj, onProperties[j]);
	    }
	    if (properties) {
	        for (var i = 0; i < properties.length; i++) {
	            patchProperty(obj, 'on' + properties[i]);
	        }
	    }
	}
	exports.patchOnProperties = patchOnProperties;
	;
	var EVENT_TASKS = exports.zoneSymbol('eventTasks');
	var ADD_EVENT_LISTENER = 'addEventListener';
	var REMOVE_EVENT_LISTENER = 'removeEventListener';
	var SYMBOL_ADD_EVENT_LISTENER = exports.zoneSymbol(ADD_EVENT_LISTENER);
	var SYMBOL_REMOVE_EVENT_LISTENER = exports.zoneSymbol(REMOVE_EVENT_LISTENER);
	function findExistingRegisteredTask(target, handler, name, capture, remove) {
	    var eventTasks = target[EVENT_TASKS];
	    if (eventTasks) {
	        for (var i = 0; i < eventTasks.length; i++) {
	            var eventTask = eventTasks[i];
	            var data = eventTask.data;
	            if (data.handler === handler
	                && data.useCapturing === capture
	                && data.eventName === name) {
	                if (remove) {
	                    eventTasks.splice(i, 1);
	                }
	                return eventTask;
	            }
	        }
	    }
	    return null;
	}
	function attachRegisteredEvent(target, eventTask) {
	    var eventTasks = target[EVENT_TASKS];
	    if (!eventTasks) {
	        eventTasks = target[EVENT_TASKS] = [];
	    }
	    eventTasks.push(eventTask);
	}
	function scheduleEventListener(eventTask) {
	    var meta = eventTask.data;
	    attachRegisteredEvent(meta.target, eventTask);
	    return meta.target[SYMBOL_ADD_EVENT_LISTENER](meta.eventName, eventTask.invoke, meta.useCapturing);
	}
	function cancelEventListener(eventTask) {
	    var meta = eventTask.data;
	    findExistingRegisteredTask(meta.target, eventTask.invoke, meta.eventName, meta.useCapturing, true);
	    meta.target[SYMBOL_REMOVE_EVENT_LISTENER](meta.eventName, eventTask.invoke, meta.useCapturing);
	}
	function zoneAwareAddEventListener(self, args) {
	    var eventName = args[0];
	    var handler = args[1];
	    var useCapturing = args[2] || false;
	    // - Inside a Web Worker, `this` is undefined, the context is `global`
	    // - When `addEventListener` is called on the global context in strict mode, `this` is undefined
	    // see https://github.com/angular/zone.js/issues/190
	    var target = self || _global;
	    var delegate = null;
	    if (typeof handler == 'function') {
	        delegate = handler;
	    }
	    else if (handler && handler.handleEvent) {
	        delegate = function (event) { return handler.handleEvent(event); };
	    }
	    var validZoneHandler = false;
	    try {
	        // In cross site contexts (such as WebDriver frameworks like Selenium),
	        // accessing the handler object here will cause an exception to be thrown which
	        // will fail tests prematurely.
	        validZoneHandler = handler && handler.toString() === "[object FunctionWrapper]";
	    }
	    catch (e) {
	        // Returning nothing here is fine, because objects in a cross-site context are unusable
	        return;
	    }
	    // Ignore special listeners of IE11 & Edge dev tools, see https://github.com/angular/zone.js/issues/150
	    if (!delegate || validZoneHandler) {
	        return target[SYMBOL_ADD_EVENT_LISTENER](eventName, handler, useCapturing);
	    }
	    var eventTask = findExistingRegisteredTask(target, handler, eventName, useCapturing, false);
	    if (eventTask) {
	        // we already registered, so this will have noop.
	        return target[SYMBOL_ADD_EVENT_LISTENER](eventName, eventTask.invoke, useCapturing);
	    }
	    var zone = Zone.current;
	    var source = target.constructor['name'] + '.addEventListener:' + eventName;
	    var data = {
	        target: target,
	        eventName: eventName,
	        name: eventName,
	        useCapturing: useCapturing,
	        handler: handler
	    };
	    zone.scheduleEventTask(source, delegate, data, scheduleEventListener, cancelEventListener);
	}
	function zoneAwareRemoveEventListener(self, args) {
	    var eventName = args[0];
	    var handler = args[1];
	    var useCapturing = args[2] || false;
	    // - Inside a Web Worker, `this` is undefined, the context is `global`
	    // - When `addEventListener` is called on the global context in strict mode, `this` is undefined
	    // see https://github.com/angular/zone.js/issues/190
	    var target = self || _global;
	    var eventTask = findExistingRegisteredTask(target, handler, eventName, useCapturing, true);
	    if (eventTask) {
	        eventTask.zone.cancelTask(eventTask);
	    }
	    else {
	        target[SYMBOL_REMOVE_EVENT_LISTENER](eventName, handler, useCapturing);
	    }
	}
	function patchEventTargetMethods(obj) {
	    if (obj && obj.addEventListener) {
	        patchMethod(obj, ADD_EVENT_LISTENER, function () { return zoneAwareAddEventListener; });
	        patchMethod(obj, REMOVE_EVENT_LISTENER, function () { return zoneAwareRemoveEventListener; });
	        return true;
	    }
	    else {
	        return false;
	    }
	}
	exports.patchEventTargetMethods = patchEventTargetMethods;
	;
	var originalInstanceKey = exports.zoneSymbol('originalInstance');
	// wrap some native API on `window`
	function patchClass(className) {
	    var OriginalClass = _global[className];
	    if (!OriginalClass)
	        return;
	    _global[className] = function () {
	        var a = bindArguments(arguments, className);
	        switch (a.length) {
	            case 0:
	                this[originalInstanceKey] = new OriginalClass();
	                break;
	            case 1:
	                this[originalInstanceKey] = new OriginalClass(a[0]);
	                break;
	            case 2:
	                this[originalInstanceKey] = new OriginalClass(a[0], a[1]);
	                break;
	            case 3:
	                this[originalInstanceKey] = new OriginalClass(a[0], a[1], a[2]);
	                break;
	            case 4:
	                this[originalInstanceKey] = new OriginalClass(a[0], a[1], a[2], a[3]);
	                break;
	            default: throw new Error('Arg list too long.');
	        }
	    };
	    var instance = new OriginalClass(function () { });
	    var prop;
	    for (prop in instance) {
	        (function (prop) {
	            if (typeof instance[prop] === 'function') {
	                _global[className].prototype[prop] = function () {
	                    return this[originalInstanceKey][prop].apply(this[originalInstanceKey], arguments);
	                };
	            }
	            else {
	                Object.defineProperty(_global[className].prototype, prop, {
	                    set: function (fn) {
	                        if (typeof fn === 'function') {
	                            this[originalInstanceKey][prop] = Zone.current.wrap(fn, className + '.' + prop);
	                        }
	                        else {
	                            this[originalInstanceKey][prop] = fn;
	                        }
	                    },
	                    get: function () {
	                        return this[originalInstanceKey][prop];
	                    }
	                });
	            }
	        }(prop));
	    }
	    for (prop in OriginalClass) {
	        if (prop !== 'prototype' && OriginalClass.hasOwnProperty(prop)) {
	            _global[className][prop] = OriginalClass[prop];
	        }
	    }
	}
	exports.patchClass = patchClass;
	;
	function createNamedFn(name, delegate) {
	    try {
	        return (Function('f', "return function " + name + "(){return f(this, arguments)}"))(delegate);
	    }
	    catch (e) {
	        // if we fail, we must be CSP, just return delegate.
	        return function () {
	            return delegate(this, arguments);
	        };
	    }
	}
	exports.createNamedFn = createNamedFn;
	function patchMethod(target, name, patchFn) {
	    var proto = target;
	    while (proto && !proto.hasOwnProperty(name)) {
	        proto = Object.getPrototypeOf(proto);
	    }
	    if (!proto && target[name]) {
	        // somehow we did not find it, but we can see it. This happens on IE for Window properties.
	        proto = target;
	    }
	    var delegateName = exports.zoneSymbol(name);
	    var delegate;
	    if (proto && !(delegate = proto[delegateName])) {
	        delegate = proto[delegateName] = proto[name];
	        proto[name] = createNamedFn(name, patchFn(delegate, delegateName, name));
	    }
	    return delegate;
	}
	exports.patchMethod = patchMethod;

	/* WEBPACK VAR INJECTION */}.call(exports, (function() { return this; }())))

/***/ },
/* 4 */
/***/ function(module, exports, __webpack_require__) {

	"use strict";
	var utils_1 = __webpack_require__(3);
	/*
	 * This is necessary for Chrome and Chrome mobile, to enable
	 * things like redefining `createdCallback` on an element.
	 */
	var _defineProperty = Object.defineProperty;
	var _getOwnPropertyDescriptor = Object.getOwnPropertyDescriptor;
	var _create = Object.create;
	var unconfigurablesKey = utils_1.zoneSymbol('unconfigurables');
	function propertyPatch() {
	    Object.defineProperty = function (obj, prop, desc) {
	        if (isUnconfigurable(obj, prop)) {
	            throw new TypeError('Cannot assign to read only property \'' + prop + '\' of ' + obj);
	        }
	        if (prop !== 'prototype') {
	            desc = rewriteDescriptor(obj, prop, desc);
	        }
	        return _defineProperty(obj, prop, desc);
	    };
	    Object.defineProperties = function (obj, props) {
	        Object.keys(props).forEach(function (prop) {
	            Object.defineProperty(obj, prop, props[prop]);
	        });
	        return obj;
	    };
	    Object.create = function (obj, proto) {
	        if (typeof proto === 'object') {
	            Object.keys(proto).forEach(function (prop) {
	                proto[prop] = rewriteDescriptor(obj, prop, proto[prop]);
	            });
	        }
	        return _create(obj, proto);
	    };
	    Object.getOwnPropertyDescriptor = function (obj, prop) {
	        var desc = _getOwnPropertyDescriptor(obj, prop);
	        if (isUnconfigurable(obj, prop)) {
	            desc.configurable = false;
	        }
	        return desc;
	    };
	}
	exports.propertyPatch = propertyPatch;
	;
	function _redefineProperty(obj, prop, desc) {
	    desc = rewriteDescriptor(obj, prop, desc);
	    return _defineProperty(obj, prop, desc);
	}
	exports._redefineProperty = _redefineProperty;
	;
	function isUnconfigurable(obj, prop) {
	    return obj && obj[unconfigurablesKey] && obj[unconfigurablesKey][prop];
	}
	function rewriteDescriptor(obj, prop, desc) {
	    desc.configurable = true;
	    if (!desc.configurable) {
	        if (!obj[unconfigurablesKey]) {
	            _defineProperty(obj, unconfigurablesKey, { writable: true, value: {} });
	        }
	        obj[unconfigurablesKey][prop] = true;
	    }
	    return desc;
	}


/***/ },
/* 5 */
/***/ function(module, exports, __webpack_require__) {

	"use strict";
	var define_property_1 = __webpack_require__(4);
	var utils_1 = __webpack_require__(3);
	function registerElementPatch(_global) {
	    if (!utils_1.isBrowser || !('registerElement' in _global.document)) {
	        return;
	    }
	    var _registerElement = document.registerElement;
	    var callbacks = [
	        'createdCallback',
	        'attachedCallback',
	        'detachedCallback',
	        'attributeChangedCallback'
	    ];
	    document.registerElement = function (name, opts) {
	        if (opts && opts.prototype) {
	            callbacks.forEach(function (callback) {
	                var source = 'Document.registerElement::' + callback;
	                if (opts.prototype.hasOwnProperty(callback)) {
	                    var descriptor = Object.getOwnPropertyDescriptor(opts.prototype, callback);
	                    if (descriptor && descriptor.value) {
	                        descriptor.value = Zone.current.wrap(descriptor.value, source);
	                        define_property_1._redefineProperty(opts.prototype, callback, descriptor);
	                    }
	                    else {
	                        opts.prototype[callback] = Zone.current.wrap(opts.prototype[callback], source);
	                    }
	                }
	                else if (opts.prototype[callback]) {
	                    opts.prototype[callback] = Zone.current.wrap(opts.prototype[callback], source);
	                }
	            });
	        }
	        return _registerElement.apply(document, [name, opts]);
	    };
	}
	exports.registerElementPatch = registerElementPatch;


/***/ },
/* 6 */
/***/ function(module, exports, __webpack_require__) {

	"use strict";
	var webSocketPatch = __webpack_require__(7);
	var utils_1 = __webpack_require__(3);
	var eventNames = 'copy cut paste abort blur focus canplay canplaythrough change click contextmenu dblclick drag dragend dragenter dragleave dragover dragstart drop durationchange emptied ended input invalid keydown keypress keyup load loadeddata loadedmetadata loadstart message mousedown mouseenter mouseleave mousemove mouseout mouseover mouseup pause play playing progress ratechange reset scroll seeked seeking select show stalled submit suspend timeupdate volumechange waiting mozfullscreenchange mozfullscreenerror mozpointerlockchange mozpointerlockerror error webglcontextrestored webglcontextlost webglcontextcreationerror'.split(' ');
	function propertyDescriptorPatch(_global) {
	    if (utils_1.isNode) {
	        return;
	    }
	    var supportsWebSocket = typeof WebSocket !== 'undefined';
	    if (canPatchViaPropertyDescriptor()) {
	        // for browsers that we can patch the descriptor:  Chrome & Firefox
	        if (utils_1.isBrowser) {
	            utils_1.patchOnProperties(HTMLElement.prototype, eventNames);
	        }
	        utils_1.patchOnProperties(XMLHttpRequest.prototype, null);
	        if (typeof IDBIndex !== 'undefined') {
	            utils_1.patchOnProperties(IDBIndex.prototype, null);
	            utils_1.patchOnProperties(IDBRequest.prototype, null);
	            utils_1.patchOnProperties(IDBOpenDBRequest.prototype, null);
	            utils_1.patchOnProperties(IDBDatabase.prototype, null);
	            utils_1.patchOnProperties(IDBTransaction.prototype, null);
	            utils_1.patchOnProperties(IDBCursor.prototype, null);
	        }
	        if (supportsWebSocket) {
	            utils_1.patchOnProperties(WebSocket.prototype, null);
	        }
	    }
	    else {
	        // Safari, Android browsers (Jelly Bean)
	        patchViaCapturingAllTheEvents();
	        utils_1.patchClass('XMLHttpRequest');
	        if (supportsWebSocket) {
	            webSocketPatch.apply(_global);
	        }
	    }
	}
	exports.propertyDescriptorPatch = propertyDescriptorPatch;
	function canPatchViaPropertyDescriptor() {
	    if (utils_1.isBrowser && !Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'onclick')
	        && typeof Element !== 'undefined') {
	        // WebKit https://bugs.webkit.org/show_bug.cgi?id=134364
	        // IDL interface attributes are not configurable
	        var desc = Object.getOwnPropertyDescriptor(Element.prototype, 'onclick');
	        if (desc && !desc.configurable)
	            return false;
	    }
	    Object.defineProperty(XMLHttpRequest.prototype, 'onreadystatechange', {
	        get: function () {
	            return true;
	        }
	    });
	    var req = new XMLHttpRequest();
	    var result = !!req.onreadystatechange;
	    Object.defineProperty(XMLHttpRequest.prototype, 'onreadystatechange', {});
	    return result;
	}
	;
	var unboundKey = utils_1.zoneSymbol('unbound');
	// Whenever any eventListener fires, we check the eventListener target and all parents
	// for `onwhatever` properties and replace them with zone-bound functions
	// - Chrome (for now)
	function patchViaCapturingAllTheEvents() {
	    var _loop_1 = function(i) {
	        var property = eventNames[i];
	        var onproperty = 'on' + property;
	        document.addEventListener(property, function (event) {
	            var elt = event.target, bound, source;
	            if (elt) {
	                source = elt.constructor['name'] + '.' + onproperty;
	            }
	            else {
	                source = 'unknown.' + onproperty;
	            }
	            while (elt) {
	                if (elt[onproperty] && !elt[onproperty][unboundKey]) {
	                    bound = Zone.current.wrap(elt[onproperty], source);
	                    bound[unboundKey] = elt[onproperty];
	                    elt[onproperty] = bound;
	                }
	                elt = elt.parentElement;
	            }
	        }, true);
	    };
	    for (var i = 0; i < eventNames.length; i++) {
	        _loop_1(i);
	    }
	    ;
	}
	;


/***/ },
/* 7 */
/***/ function(module, exports, __webpack_require__) {

	"use strict";
	var utils_1 = __webpack_require__(3);
	// we have to patch the instance since the proto is non-configurable
	function apply(_global) {
	    var WS = _global.WebSocket;
	    // On Safari window.EventTarget doesn't exist so need to patch WS add/removeEventListener
	    // On older Chrome, no need since EventTarget was already patched
	    if (!_global.EventTarget) {
	        utils_1.patchEventTargetMethods(WS.prototype);
	    }
	    _global.WebSocket = function (a, b) {
	        var socket = arguments.length > 1 ? new WS(a, b) : new WS(a);
	        var proxySocket;
	        // Safari 7.0 has non-configurable own 'onmessage' and friends properties on the socket instance
	        var onmessageDesc = Object.getOwnPropertyDescriptor(socket, 'onmessage');
	        if (onmessageDesc && onmessageDesc.configurable === false) {
	            proxySocket = Object.create(socket);
	            ['addEventListener', 'removeEventListener', 'send', 'close'].forEach(function (propName) {
	                proxySocket[propName] = function () {
	                    return socket[propName].apply(socket, arguments);
	                };
	            });
	        }
	        else {
	            // we can patch the real socket
	            proxySocket = socket;
	        }
	        utils_1.patchOnProperties(proxySocket, ['close', 'error', 'message', 'open']);
	        return proxySocket;
	    };
	    for (var prop in WS) {
	        _global.WebSocket[prop] = WS[prop];
	    }
	}
	exports.apply = apply;


/***/ },
/* 8 */
/***/ function(module, exports, __webpack_require__) {

	"use strict";
	var utils_1 = __webpack_require__(3);
	function patchTimer(window, setName, cancelName, nameSuffix) {
	    var setNative = null;
	    var clearNative = null;
	    setName += nameSuffix;
	    cancelName += nameSuffix;
	    function scheduleTask(task) {
	        var data = task.data;
	        data.args[0] = task.invoke;
	        data.handleId = setNative.apply(window, data.args);
	        return task;
	    }
	    function clearTask(task) {
	        return clearNative(task.data.handleId);
	    }
	    setNative = utils_1.patchMethod(window, setName, function (delegate) { return function (self, args) {
	        if (typeof args[0] === 'function') {
	            var zone = Zone.current;
	            var options = {
	                handleId: null,
	                isPeriodic: nameSuffix === 'Interval',
	                delay: (nameSuffix === 'Timeout' || nameSuffix === 'Interval') ? args[1] || 0 : null,
	                args: args
	            };
	            return zone.scheduleMacroTask(setName, args[0], options, scheduleTask, clearTask);
	        }
	        else {
	            // cause an error by calling it directly.
	            return delegate.apply(window, args);
	        }
	    }; });
	    clearNative = utils_1.patchMethod(window, cancelName, function (delegate) { return function (self, args) {
	        var task = args[0];
	        if (task && typeof task.type === 'string') {
	            if (task.cancelFn && task.data.isPeriodic || task.runCount === 0) {
	                // Do not cancel already canceled functions
	                task.zone.cancelTask(task);
	            }
	        }
	        else {
	            // cause an error by calling it directly.
	            delegate.apply(window, args);
	        }
	    }; });
	}
	exports.patchTimer = patchTimer;


/***/ }
/******/ ]);
/*! *****************************************************************************
Copyright (C) Microsoft. All rights reserved.
Licensed under the Apache License, Version 2.0 (the "License"); you may not use
this file except in compliance with the License. You may obtain a copy of the
License at http://www.apache.org/licenses/LICENSE-2.0

THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABLITY OR NON-INFRINGEMENT.

See the Apache Version 2.0 License for specific language governing permissions
and limitations under the License.
***************************************************************************** */
var Reflect;
(function (Reflect) {
    "use strict";
    // Load global or shim versions of Map, Set, and WeakMap
    var functionPrototype = Object.getPrototypeOf(Function);
    var _Map = typeof Map === "function" ? Map : CreateMapPolyfill();
    var _Set = typeof Set === "function" ? Set : CreateSetPolyfill();
    var _WeakMap = typeof WeakMap === "function" ? WeakMap : CreateWeakMapPolyfill();
    // [[Metadata]] internal slot
    var __Metadata__ = new _WeakMap();
    /**
      * Applies a set of decorators to a property of a target object.
      * @param decorators An array of decorators.
      * @param target The target object.
      * @param targetKey (Optional) The property key to decorate.
      * @param targetDescriptor (Optional) The property descriptor for the target key
      * @remarks Decorators are applied in reverse order.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     C = Reflect.decorate(decoratorsArray, C);
      *
      *     // property (on constructor)
      *     Reflect.decorate(decoratorsArray, C, "staticProperty");
      *
      *     // property (on prototype)
      *     Reflect.decorate(decoratorsArray, C.prototype, "property");
      *
      *     // method (on constructor)
      *     Object.defineProperty(C, "staticMethod",
      *         Reflect.decorate(decoratorsArray, C, "staticMethod",
      *             Object.getOwnPropertyDescriptor(C, "staticMethod")));
      *
      *     // method (on prototype)
      *     Object.defineProperty(C.prototype, "method",
      *         Reflect.decorate(decoratorsArray, C.prototype, "method",
      *             Object.getOwnPropertyDescriptor(C.prototype, "method")));
      *
      */
    function decorate(decorators, target, targetKey, targetDescriptor) {
        if (!IsUndefined(targetDescriptor)) {
            if (!IsArray(decorators)) {
                throw new TypeError();
            }
            else if (!IsObject(target)) {
                throw new TypeError();
            }
            else if (IsUndefined(targetKey)) {
                throw new TypeError();
            }
            else if (!IsObject(targetDescriptor)) {
                throw new TypeError();
            }
            targetKey = ToPropertyKey(targetKey);
            return DecoratePropertyWithDescriptor(decorators, target, targetKey, targetDescriptor);
        }
        else if (!IsUndefined(targetKey)) {
            if (!IsArray(decorators)) {
                throw new TypeError();
            }
            else if (!IsObject(target)) {
                throw new TypeError();
            }
            targetKey = ToPropertyKey(targetKey);
            return DecoratePropertyWithoutDescriptor(decorators, target, targetKey);
        }
        else {
            if (!IsArray(decorators)) {
                throw new TypeError();
            }
            else if (!IsConstructor(target)) {
                throw new TypeError();
            }
            return DecorateConstructor(decorators, target);
        }
    }
    Reflect.decorate = decorate;
    /**
      * A default metadata decorator factory that can be used on a class, class member, or parameter.
      * @param metadataKey The key for the metadata entry.
      * @param metadataValue The value for the metadata entry.
      * @returns A decorator function.
      * @remarks
      * If `metadataKey` is already defined for the target and target key, the
      * metadataValue for that key will be overwritten.
      * @example
      *
      *     // constructor
      *     @Reflect.metadata(key, value)
      *     class C {
      *     }
      *
      *     // property (on constructor, TypeScript only)
      *     class C {
      *         @Reflect.metadata(key, value)
      *         static staticProperty;
      *     }
      *
      *     // property (on prototype, TypeScript only)
      *     class C {
      *         @Reflect.metadata(key, value)
      *         property;
      *     }
      *
      *     // method (on constructor)
      *     class C {
      *         @Reflect.metadata(key, value)
      *         static staticMethod() { }
      *     }
      *
      *     // method (on prototype)
      *     class C {
      *         @Reflect.metadata(key, value)
      *         method() { }
      *     }
      *
      */
    function metadata(metadataKey, metadataValue) {
        function decorator(target, targetKey) {
            if (!IsUndefined(targetKey)) {
                if (!IsObject(target)) {
                    throw new TypeError();
                }
                targetKey = ToPropertyKey(targetKey);
                OrdinaryDefineOwnMetadata(metadataKey, metadataValue, target, targetKey);
            }
            else {
                if (!IsConstructor(target)) {
                    throw new TypeError();
                }
                OrdinaryDefineOwnMetadata(metadataKey, metadataValue, target, /*targetKey*/ undefined);
            }
        }
        return decorator;
    }
    Reflect.metadata = metadata;
    /**
      * Define a unique metadata entry on the target.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param metadataValue A value that contains attached metadata.
      * @param target The target object on which to define metadata.
      * @param targetKey (Optional) The property key for the target.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     Reflect.defineMetadata("custom:annotation", options, C);
      *
      *     // property (on constructor)
      *     Reflect.defineMetadata("custom:annotation", options, C, "staticProperty");
      *
      *     // property (on prototype)
      *     Reflect.defineMetadata("custom:annotation", options, C.prototype, "property");
      *
      *     // method (on constructor)
      *     Reflect.defineMetadata("custom:annotation", options, C, "staticMethod");
      *
      *     // method (on prototype)
      *     Reflect.defineMetadata("custom:annotation", options, C.prototype, "method");
      *
      *     // decorator factory as metadata-producing annotation.
      *     function MyAnnotation(options): Decorator {
      *         return (target, key?) => Reflect.defineMetadata("custom:annotation", options, target, key);
      *     }
      *
      */
    function defineMetadata(metadataKey, metadataValue, target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        return OrdinaryDefineOwnMetadata(metadataKey, metadataValue, target, targetKey);
    }
    Reflect.defineMetadata = defineMetadata;
    /**
      * Gets a value indicating whether the target object or its prototype chain has the provided metadata key defined.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param targetKey (Optional) The property key for the target.
      * @returns `true` if the metadata key was defined on the target object or its prototype chain; otherwise, `false`.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.hasMetadata("custom:annotation", C);
      *
      *     // property (on constructor)
      *     result = Reflect.hasMetadata("custom:annotation", C, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.hasMetadata("custom:annotation", C.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.hasMetadata("custom:annotation", C, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.hasMetadata("custom:annotation", C.prototype, "method");
      *
      */
    function hasMetadata(metadataKey, target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        return OrdinaryHasMetadata(metadataKey, target, targetKey);
    }
    Reflect.hasMetadata = hasMetadata;
    /**
      * Gets a value indicating whether the target object has the provided metadata key defined.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param targetKey (Optional) The property key for the target.
      * @returns `true` if the metadata key was defined on the target object; otherwise, `false`.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.hasOwnMetadata("custom:annotation", C);
      *
      *     // property (on constructor)
      *     result = Reflect.hasOwnMetadata("custom:annotation", C, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.hasOwnMetadata("custom:annotation", C.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.hasOwnMetadata("custom:annotation", C, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.hasOwnMetadata("custom:annotation", C.prototype, "method");
      *
      */
    function hasOwnMetadata(metadataKey, target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        return OrdinaryHasOwnMetadata(metadataKey, target, targetKey);
    }
    Reflect.hasOwnMetadata = hasOwnMetadata;
    /**
      * Gets the metadata value for the provided metadata key on the target object or its prototype chain.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param targetKey (Optional) The property key for the target.
      * @returns The metadata value for the metadata key if found; otherwise, `undefined`.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getMetadata("custom:annotation", C);
      *
      *     // property (on constructor)
      *     result = Reflect.getMetadata("custom:annotation", C, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getMetadata("custom:annotation", C.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getMetadata("custom:annotation", C, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getMetadata("custom:annotation", C.prototype, "method");
      *
      */
    function getMetadata(metadataKey, target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        return OrdinaryGetMetadata(metadataKey, target, targetKey);
    }
    Reflect.getMetadata = getMetadata;
    /**
      * Gets the metadata value for the provided metadata key on the target object.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param targetKey (Optional) The property key for the target.
      * @returns The metadata value for the metadata key if found; otherwise, `undefined`.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getOwnMetadata("custom:annotation", C);
      *
      *     // property (on constructor)
      *     result = Reflect.getOwnMetadata("custom:annotation", C, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getOwnMetadata("custom:annotation", C.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getOwnMetadata("custom:annotation", C, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getOwnMetadata("custom:annotation", C.prototype, "method");
      *
      */
    function getOwnMetadata(metadataKey, target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        return OrdinaryGetOwnMetadata(metadataKey, target, targetKey);
    }
    Reflect.getOwnMetadata = getOwnMetadata;
    /**
      * Gets the metadata keys defined on the target object or its prototype chain.
      * @param target The target object on which the metadata is defined.
      * @param targetKey (Optional) The property key for the target.
      * @returns An array of unique metadata keys.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getMetadataKeys(C);
      *
      *     // property (on constructor)
      *     result = Reflect.getMetadataKeys(C, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getMetadataKeys(C.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getMetadataKeys(C, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getMetadataKeys(C.prototype, "method");
      *
      */
    function getMetadataKeys(target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        return OrdinaryMetadataKeys(target, targetKey);
    }
    Reflect.getMetadataKeys = getMetadataKeys;
    /**
      * Gets the unique metadata keys defined on the target object.
      * @param target The target object on which the metadata is defined.
      * @param targetKey (Optional) The property key for the target.
      * @returns An array of unique metadata keys.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.getOwnMetadataKeys(C);
      *
      *     // property (on constructor)
      *     result = Reflect.getOwnMetadataKeys(C, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.getOwnMetadataKeys(C.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.getOwnMetadataKeys(C, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.getOwnMetadataKeys(C.prototype, "method");
      *
      */
    function getOwnMetadataKeys(target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        return OrdinaryOwnMetadataKeys(target, targetKey);
    }
    Reflect.getOwnMetadataKeys = getOwnMetadataKeys;
    /**
      * Deletes the metadata entry from the target object with the provided key.
      * @param metadataKey A key used to store and retrieve metadata.
      * @param target The target object on which the metadata is defined.
      * @param targetKey (Optional) The property key for the target.
      * @returns `true` if the metadata entry was found and deleted; otherwise, false.
      * @example
      *
      *     class C {
      *         // property declarations are not part of ES6, though they are valid in TypeScript:
      *         // static staticProperty;
      *         // property;
      *
      *         constructor(p) { }
      *         static staticMethod(p) { }
      *         method(p) { }
      *     }
      *
      *     // constructor
      *     result = Reflect.deleteMetadata("custom:annotation", C);
      *
      *     // property (on constructor)
      *     result = Reflect.deleteMetadata("custom:annotation", C, "staticProperty");
      *
      *     // property (on prototype)
      *     result = Reflect.deleteMetadata("custom:annotation", C.prototype, "property");
      *
      *     // method (on constructor)
      *     result = Reflect.deleteMetadata("custom:annotation", C, "staticMethod");
      *
      *     // method (on prototype)
      *     result = Reflect.deleteMetadata("custom:annotation", C.prototype, "method");
      *
      */
    function deleteMetadata(metadataKey, target, targetKey) {
        if (!IsObject(target)) {
            throw new TypeError();
        }
        else if (!IsUndefined(targetKey)) {
            targetKey = ToPropertyKey(targetKey);
        }
        // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#deletemetadata-metadatakey-p-
        var metadataMap = GetOrCreateMetadataMap(target, targetKey, /*create*/ false);
        if (IsUndefined(metadataMap)) {
            return false;
        }
        if (!metadataMap.delete(metadataKey)) {
            return false;
        }
        if (metadataMap.size > 0) {
            return true;
        }
        var targetMetadata = __Metadata__.get(target);
        targetMetadata.delete(targetKey);
        if (targetMetadata.size > 0) {
            return true;
        }
        __Metadata__.delete(target);
        return true;
    }
    Reflect.deleteMetadata = deleteMetadata;
    function DecorateConstructor(decorators, target) {
        for (var i = decorators.length - 1; i >= 0; --i) {
            var decorator = decorators[i];
            var decorated = decorator(target);
            if (!IsUndefined(decorated)) {
                if (!IsConstructor(decorated)) {
                    throw new TypeError();
                }
                target = decorated;
            }
        }
        return target;
    }
    function DecoratePropertyWithDescriptor(decorators, target, propertyKey, descriptor) {
        for (var i = decorators.length - 1; i >= 0; --i) {
            var decorator = decorators[i];
            var decorated = decorator(target, propertyKey, descriptor);
            if (!IsUndefined(decorated)) {
                if (!IsObject(decorated)) {
                    throw new TypeError();
                }
                descriptor = decorated;
            }
        }
        return descriptor;
    }
    function DecoratePropertyWithoutDescriptor(decorators, target, propertyKey) {
        for (var i = decorators.length - 1; i >= 0; --i) {
            var decorator = decorators[i];
            decorator(target, propertyKey);
        }
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#getorcreatemetadatamap--o-p-create-
    function GetOrCreateMetadataMap(target, targetKey, create) {
        var targetMetadata = __Metadata__.get(target);
        if (!targetMetadata) {
            if (!create) {
                return undefined;
            }
            targetMetadata = new _Map();
            __Metadata__.set(target, targetMetadata);
        }
        var keyMetadata = targetMetadata.get(targetKey);
        if (!keyMetadata) {
            if (!create) {
                return undefined;
            }
            keyMetadata = new _Map();
            targetMetadata.set(targetKey, keyMetadata);
        }
        return keyMetadata;
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#ordinaryhasmetadata--metadatakey-o-p-
    function OrdinaryHasMetadata(MetadataKey, O, P) {
        var hasOwn = OrdinaryHasOwnMetadata(MetadataKey, O, P);
        if (hasOwn) {
            return true;
        }
        var parent = GetPrototypeOf(O);
        if (parent !== null) {
            return OrdinaryHasMetadata(MetadataKey, parent, P);
        }
        return false;
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#ordinaryhasownmetadata--metadatakey-o-p-
    function OrdinaryHasOwnMetadata(MetadataKey, O, P) {
        var metadataMap = GetOrCreateMetadataMap(O, P, /*create*/ false);
        if (metadataMap === undefined) {
            return false;
        }
        return Boolean(metadataMap.has(MetadataKey));
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#ordinarygetmetadata--metadatakey-o-p-
    function OrdinaryGetMetadata(MetadataKey, O, P) {
        var hasOwn = OrdinaryHasOwnMetadata(MetadataKey, O, P);
        if (hasOwn) {
            return OrdinaryGetOwnMetadata(MetadataKey, O, P);
        }
        var parent = GetPrototypeOf(O);
        if (parent !== null) {
            return OrdinaryGetMetadata(MetadataKey, parent, P);
        }
        return undefined;
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#ordinarygetownmetadata--metadatakey-o-p-
    function OrdinaryGetOwnMetadata(MetadataKey, O, P) {
        var metadataMap = GetOrCreateMetadataMap(O, P, /*create*/ false);
        if (metadataMap === undefined) {
            return undefined;
        }
        return metadataMap.get(MetadataKey);
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#ordinarydefineownmetadata--metadatakey-metadatavalue-o-p-
    function OrdinaryDefineOwnMetadata(MetadataKey, MetadataValue, O, P) {
        var metadataMap = GetOrCreateMetadataMap(O, P, /*create*/ true);
        metadataMap.set(MetadataKey, MetadataValue);
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#ordinarymetadatakeys--o-p-
    function OrdinaryMetadataKeys(O, P) {
        var ownKeys = OrdinaryOwnMetadataKeys(O, P);
        var parent = GetPrototypeOf(O);
        if (parent === null) {
            return ownKeys;
        }
        var parentKeys = OrdinaryMetadataKeys(parent, P);
        if (parentKeys.length <= 0) {
            return ownKeys;
        }
        if (ownKeys.length <= 0) {
            return parentKeys;
        }
        var set = new _Set();
        var keys = [];
        for (var _i = 0; _i < ownKeys.length; _i++) {
            var key = ownKeys[_i];
            var hasKey = set.has(key);
            if (!hasKey) {
                set.add(key);
                keys.push(key);
            }
        }
        for (var _a = 0; _a < parentKeys.length; _a++) {
            var key = parentKeys[_a];
            var hasKey = set.has(key);
            if (!hasKey) {
                set.add(key);
                keys.push(key);
            }
        }
        return keys;
    }
    // https://github.com/jonathandturner/decorators/blob/master/specs/metadata.md#ordinaryownmetadatakeys--o-p-
    function OrdinaryOwnMetadataKeys(target, targetKey) {
        var metadataMap = GetOrCreateMetadataMap(target, targetKey, /*create*/ false);
        var keys = [];
        if (metadataMap) {
            metadataMap.forEach(function (_, key) { return keys.push(key); });
        }
        return keys;
    }
    // https://people.mozilla.org/~jorendorff/es6-draft.html#sec-ecmascript-language-types-undefined-type
    function IsUndefined(x) {
        return x === undefined;
    }
    // https://people.mozilla.org/~jorendorff/es6-draft.html#sec-isarray
    function IsArray(x) {
        return Array.isArray(x);
    }
    // https://people.mozilla.org/~jorendorff/es6-draft.html#sec-object-type
    function IsObject(x) {
        return typeof x === "object" ? x !== null : typeof x === "function";
    }
    // https://people.mozilla.org/~jorendorff/es6-draft.html#sec-isconstructor
    function IsConstructor(x) {
        return typeof x === "function";
    }
    // https://people.mozilla.org/~jorendorff/es6-draft.html#sec-ecmascript-language-types-symbol-type
    function IsSymbol(x) {
        return typeof x === "symbol";
    }
    // https://people.mozilla.org/~jorendorff/es6-draft.html#sec-topropertykey
    function ToPropertyKey(value) {
        if (IsSymbol(value)) {
            return value;
        }
        return String(value);
    }
    function GetPrototypeOf(O) {
        var proto = Object.getPrototypeOf(O);
        if (typeof O !== "function" || O === functionPrototype) {
            return proto;
        }
        // TypeScript doesn't set __proto__ in ES5, as it's non-standard. 
        // Try to determine the superclass constructor. Compatible implementations
        // must either set __proto__ on a subclass constructor to the superclass constructor,
        // or ensure each class has a valid `constructor` property on its prototype that
        // points back to the constructor.
        // If this is not the same as Function.[[Prototype]], then this is definately inherited.
        // This is the case when in ES6 or when using __proto__ in a compatible browser.
        if (proto !== functionPrototype) {
            return proto;
        }
        // If the super prototype is Object.prototype, null, or undefined, then we cannot determine the heritage.
        var prototype = O.prototype;
        var prototypeProto = Object.getPrototypeOf(prototype);
        if (prototypeProto == null || prototypeProto === Object.prototype) {
            return proto;
        }
        // if the constructor was not a function, then we cannot determine the heritage.
        var constructor = prototypeProto.constructor;
        if (typeof constructor !== "function") {
            return proto;
        }
        // if we have some kind of self-reference, then we cannot determine the heritage.
        if (constructor === O) {
            return proto;
        }
        // we have a pretty good guess at the heritage.
        return constructor;
    }
    // naive Map shim
    function CreateMapPolyfill() {
        var cacheSentinel = {};
        function Map() {
            this._keys = [];
            this._values = [];
            this._cache = cacheSentinel;
        }
        Map.prototype = {
            get size() {
                return this._keys.length;
            },
            has: function (key) {
                if (key === this._cache) {
                    return true;
                }
                if (this._find(key) >= 0) {
                    this._cache = key;
                    return true;
                }
                return false;
            },
            get: function (key) {
                var index = this._find(key);
                if (index >= 0) {
                    this._cache = key;
                    return this._values[index];
                }
                return undefined;
            },
            set: function (key, value) {
                this.delete(key);
                this._keys.push(key);
                this._values.push(value);
                this._cache = key;
                return this;
            },
            delete: function (key) {
                var index = this._find(key);
                if (index >= 0) {
                    this._keys.splice(index, 1);
                    this._values.splice(index, 1);
                    this._cache = cacheSentinel;
                    return true;
                }
                return false;
            },
            clear: function () {
                this._keys.length = 0;
                this._values.length = 0;
                this._cache = cacheSentinel;
            },
            forEach: function (callback, thisArg) {
                var size = this.size;
                for (var i = 0; i < size; ++i) {
                    var key = this._keys[i];
                    var value = this._values[i];
                    this._cache = key;
                    callback.call(this, value, key, this);
                }
            },
            _find: function (key) {
                var keys = this._keys;
                var size = keys.length;
                for (var i = 0; i < size; ++i) {
                    if (keys[i] === key) {
                        return i;
                    }
                }
                return -1;
            }
        };
        return Map;
    }
    // naive Set shim
    function CreateSetPolyfill() {
        var cacheSentinel = {};
        function Set() {
            this._map = new _Map();
        }
        Set.prototype = {
            get size() {
                return this._map.length;
            },
            has: function (value) {
                return this._map.has(value);
            },
            add: function (value) {
                this._map.set(value, value);
                return this;
            },
            delete: function (value) {
                return this._map.delete(value);
            },
            clear: function () {
                this._map.clear();
            },
            forEach: function (callback, thisArg) {
                this._map.forEach(callback, thisArg);
            }
        };
        return Set;
    }
    // naive WeakMap shim
    function CreateWeakMapPolyfill() {
        var UUID_SIZE = 16;
        var isNode = typeof global !== "undefined" && Object.prototype.toString.call(global.process) === '[object process]';
        var nodeCrypto = isNode && require("crypto");
        var hasOwn = Object.prototype.hasOwnProperty;
        var keys = {};
        var rootKey = CreateUniqueKey();
        function WeakMap() {
            this._key = CreateUniqueKey();
        }
        WeakMap.prototype = {
            has: function (target) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ false);
                if (table) {
                    return this._key in table;
                }
                return false;
            },
            get: function (target) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ false);
                if (table) {
                    return table[this._key];
                }
                return undefined;
            },
            set: function (target, value) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ true);
                table[this._key] = value;
                return this;
            },
            delete: function (target) {
                var table = GetOrCreateWeakMapTable(target, /*create*/ false);
                if (table && this._key in table) {
                    return delete table[this._key];
                }
                return false;
            },
            clear: function () {
                // NOTE: not a real clear, just makes the previous data unreachable
                this._key = CreateUniqueKey();
            }
        };
        function FillRandomBytes(buffer, size) {
            for (var i = 0; i < size; ++i) {
                buffer[i] = Math.random() * 255 | 0;
            }
        }
        function GenRandomBytes(size) {
            if (nodeCrypto) {
                var data = nodeCrypto.randomBytes(size);
                return data;
            }
            else if (typeof Uint8Array === "function") {
                var data = new Uint8Array(size);
                if (typeof crypto !== "undefined") {
                    crypto.getRandomValues(data);
                }
                else if (typeof msCrypto !== "undefined") {
                    msCrypto.getRandomValues(data);
                }
                else {
                    FillRandomBytes(data, size);
                }
                return data;
            }
            else {
                var data = new Array(size);
                FillRandomBytes(data, size);
                return data;
            }
        }
        function CreateUUID() {
            var data = GenRandomBytes(UUID_SIZE);
            // mark as random - RFC 4122 § 4.4
            data[6] = data[6] & 0x4f | 0x40;
            data[8] = data[8] & 0xbf | 0x80;
            var result = "";
            for (var offset = 0; offset < UUID_SIZE; ++offset) {
                var byte = data[offset];
                if (offset === 4 || offset === 6 || offset === 8) {
                    result += "-";
                }
                if (byte < 16) {
                    result += "0";
                }
                result += byte.toString(16).toLowerCase();
            }
            return result;
        }
        function CreateUniqueKey() {
            var key;
            do {
                key = "@@WeakMap@@" + CreateUUID();
            } while (hasOwn.call(keys, key));
            keys[key] = true;
            return key;
        }
        function GetOrCreateWeakMapTable(target, create) {
            if (!hasOwn.call(target, rootKey)) {
                if (!create) {
                    return undefined;
                }
                Object.defineProperty(target, rootKey, { value: Object.create(null) });
            }
            return target[rootKey];
        }
        return WeakMap;
    }
    // hook global Reflect
    (function (__global) {
        if (typeof __global.Reflect !== "undefined") {
            if (__global.Reflect !== Reflect) {
                for (var p in Reflect) {
                    __global.Reflect[p] = Reflect[p];
                }
            }
        }
        else {
            __global.Reflect = Reflect;
        }
    })(typeof window !== "undefined" ? window :
        typeof WorkerGlobalScope !== "undefined" ? self :
            typeof global !== "undefined" ? global :
                Function("return this;")());
})(Reflect || (Reflect = {}));
//# sourceMappingURL=Reflect.js.map
"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const overlay_service_1 = require('./services/overlay.service');
const tab_list_component_1 = require('./components/tab-list.component');
const connection_manager_component_1 = require('./components/connection-manager.component');
const tab_view_component_1 = require('./components/tab-view.component');
let AppComponent = class AppComponent {
    constructor(overlayService) {
        this.connectionsVisible = false;
        overlayService
            .connections
            .subscribe(visible => {
            this.connectionsVisible = visible;
        });
    }
};
AppComponent = __decorate([
    core_1.Component({
        selector: 'f-app',
        directives: [
            tab_list_component_1.TabListComponent, connection_manager_component_1.ConnectionManagerComponent, tab_view_component_1.TabViewComponent
        ],
        template: `
<div class="main-layer">
    <f-tab-list></f-tab-list>
    <f-tab-view></f-tab-view>
</div>
<div class="main-cover {{connectionsVisible ? 'layer-visible' : ''}}">
</div>
<div class="over-layer {{connectionsVisible ? 'layer-visible' : ''}}">
    <f-connection-manager></f-connection-manager>
</div>
`
    }), 
    __metadata('design:paramtypes', [overlay_service_1.OverlayService])
], AppComponent);
exports.AppComponent = AppComponent;

"use strict";
const path = electronRequire('path');
const omnisharpPath = path.resolve((IS_LINUX ? `${process.env.HOME}/.linq-editor/` :
    `${process.env.LOCALAPPDATA}\\LinqEditor\\`) + 'omnisharp');
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = {
    omnisharpPort: 2000,
    queryEnginePort: 8111,
    omnisharpProjectPath: omnisharpPath,
    dotnetDebugPath: IS_LINUX ? path.normalize('/usr/bin/dotnet')
        : path.normalize('C:/Program Files/dotnet/dotnet.exe')
};

"use strict";
const core_1 = require('@angular/core');
if (MODE === 'PRODUCTION') {
    core_1.enableProdMode();
}
else {
    console.log(MODE);
}
const platform_browser_dynamic_1 = require('@angular/platform-browser-dynamic');
const http_1 = require('@angular/http');
const app_component_1 = require('./app.component');
const monitor_service_1 = require('./services/monitor.service');
const overlay_service_1 = require('./services/overlay.service');
const storage_service_1 = require('./services/storage.service');
const connection_service_1 = require('./services/connection.service');
const tab_service_1 = require('./services/tab.service');
const editor_service_1 = require('./services/editor.service');
const query_service_1 = require('./services/query.service');
const omnisharp_service_1 = require('./services/omnisharp.service');
const log_service_1 = require('./services/log.service');
const mirror_change_stream_1 = require('./services/mirror-change.stream');
const hotkey_service_1 = require('./services/hotkey.service');
platform_browser_dynamic_1.bootstrap(app_component_1.AppComponent, [
    monitor_service_1.MonitorService,
    overlay_service_1.OverlayService,
    storage_service_1.StorageService,
    connection_service_1.ConnectionService,
    tab_service_1.TabService,
    editor_service_1.EditorService,
    query_service_1.QueryService,
    omnisharp_service_1.OmnisharpService,
    log_service_1.LogService,
    mirror_change_stream_1.MirrorChangeStream,
    tab_service_1.TabService,
    hotkey_service_1.HotkeyService,
    http_1.HTTP_PROVIDERS
]);

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const overlay_service_1 = require('../services/overlay.service');
const connection_service_1 = require('../services/connection.service');
const tab_service_1 = require('../services/tab.service');
const connection_1 = require('../models/connection');
let ConnectionManagerComponent = class ConnectionManagerComponent {
    constructor(connectionManager, conns, tabService) {
        this.connectionManager = connectionManager;
        this.conns = conns;
        this.tabService = tabService;
        conns.all
            .subscribe(cs => {
            this.connections = cs;
            this.connectionsSubTitle = cs.length > 0 ?
                'Current connections' : '<i>No connections</i>';
        });
    }
    closeManager() {
        // const isStart = location.hash.indexOf('/start') !== -1;
        // if (isStart) {
        //     const conn = this.connectionService.defaultConnection;
        //     if (conn) {
        //         this.tabService.newForeground(conn);
        //     }
        // }
        this.connectionManager.hideConnections();
    }
    addNewConnection(value) {
        if (value.length > 0) {
            this.conns.add(new connection_1.Connection(value));
            this.newConnectionStringText = '';
        }
    }
    editConnection(connection) {
        connection.temporary = connection.connectionString;
        connection.editing = true;
    }
    removeConnection(connection) {
        this.conns.remove(connection);
    }
    stopEditing(connection, value) {
        connection.temporary = value;
    }
    updateEditing(connection, value) {
        connection.connectionString = value;
        this.cancelEditing(connection);
        this.conns.update(connection);
    }
    cancelEditing(connection) {
        connection.editing = false;
        connection.temporary = null;
    }
};
ConnectionManagerComponent = __decorate([
    core_1.Component({
        selector: 'f-connection-manager',
        template: `
<div class="container-fluid int-test-conn-man" style="background:transparent">
    <div class="jumbotron center-block">
        <div class="row">
            <div class="col-md-12">
                <h2>Connection Manager</h2>
            </div>
        </div>
        <form>
            <div class="form-group">
                <label for="connectringStringInp">Add new</label>
                <input type="string" class="form-control" 
                    id="connectringStringInp" placeholder="Type/paste connection string and press enter to add"
                    #newconnection [(ngModel)]="newConnectionStringText"
                    (keyup.enter)="addNewConnection(newconnection.value)">
            </div>
        </form>
        <div class="row">
            <div class="col-md-12">
                <table class="table">
                    <thead><caption style="white-space: pre" [innerHTML]="connectionsSubTitle"></caption></thead>
                    <tbody>
                        <tr *ngFor="let conn of connections">
                            <td style="vertical-align: middle">
                                <p *ngIf="!conn.editing" style="margin-bottom: 0">
                                    <span (dblclick)="editConnection(conn)" 
                                        style="font-size: 80%;"
                                        title="Double-click to edit">{{conn.connectionString}}</span>
                                    
                                </p>
                                <p *ngIf="conn.editing" style="margin-bottom: 0">
                                    <input #editedconn class="form-control"
                                        [value]="conn.temporary" 
                                        (blur)="stopEditing(conn, editedconn.value)" 
                                        (keyup.enter)="updateEditing(conn, editedconn.value)" 
                                        (keyup.escape)="cancelEditing(conn)">
                                </p>
                            </td>
                            <td>
                                <button (click)="removeConnection(conn)" class="btn btn-default pull-right">Remove</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12">
                <p><button type="button" (click)="closeManager()" class="btn btn-default">Close</button></p>
            </div>
        </div>
    </div>
</div>
`
    }), 
    __metadata('design:paramtypes', [overlay_service_1.OverlayService, connection_service_1.ConnectionService, tab_service_1.TabService])
], ConnectionManagerComponent);
exports.ConnectionManagerComponent = ConnectionManagerComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const ng2_bootstrap_1 = require('ng2-bootstrap/ng2-bootstrap');
const connection_service_1 = require('../services/connection.service');
const tab_service_1 = require('../services/tab.service');
let ConnectionSelectorComponent = class ConnectionSelectorComponent {
    constructor(conns, tabs) {
        this.conns = conns;
        this.tabs = tabs;
        this.tabId = null;
        this.connId = null;
        this.connections = [];
        tabs.active
            .subscribe(tab => {
            this.connId = tab.connectionId;
            this.tabId = tab.id;
        });
        conns.all
            .subscribe(cs => {
            this.connections = cs;
        });
    }
    getText() {
        const elm = this.connections.find(x => x.id === this.connId);
        return elm && elm.connectionString || '';
    }
    select(conn) {
        this.tabs.setConnection(this.tabId, conn);
    }
};
ConnectionSelectorComponent = __decorate([
    core_1.Component({
        selector: 'f-connection-selector',
        directives: [ng2_bootstrap_1.DROPDOWN_DIRECTIVES],
        template: `
<div class="input-group int-test-conn-sel" (click)="$event.preventDefault()">
    <div class="input-group-btn btn-group" dropdown keyboardNav="true">
        <button id="connection-selector-btn-keyboard-nav" type="button" class="btn btn-default" dropdownToggle>
            Connection <span class="caret"></span>
        </button>
        <ul ref="menulist" class="dropdown-menu" role="menu" aria-labelledby="connection-selector-btn-keyboard-nav">
            <li role="menuitem" *ngFor="let conn of connections">
                <a class="dropdown-item" href="javascript:void(0)" (click)="select(conn)">{{conn.connectionString}}</a>
            </li>
        </ul>
    </div>
    <div class="form-control" style="overflow: hidden">
        <span>{{getText()}}</span>
    </div>
</div>
`
    }), 
    __metadata('design:paramtypes', [connection_service_1.ConnectionService, tab_service_1.TabService])
], ConnectionSelectorComponent);
exports.ConnectionSelectorComponent = ConnectionSelectorComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const mirror_change_stream_1 = require('../services/mirror-change.stream');
const monitor_service_1 = require('../services/monitor.service');
const hotkey_service_1 = require('../services/hotkey.service');
let ExecuteQueryComponent = class ExecuteQueryComponent {
    constructor(mirrors, monitorService, hotkeys) {
        this.mirrors = mirrors;
        this.isDisabled = true;
        this.isExecuting = false;
        monitorService.queryReady.then(() => {
            this.isDisabled = false;
        });
        hotkeys.executeQuery
            .subscribe(() => this.run());
    }
    get isEnabled() {
        return !this.isDisabled && !this.isExecuting;
    }
    run() {
        if (!this.isEnabled) {
            return;
        }
        else {
            this.isExecuting = true;
            this.mirrors.execute();
            setTimeout(() => {
                this.isExecuting = false;
            }, 1000); // disable spamming
        }
    }
};
ExecuteQueryComponent = __decorate([
    core_1.Component({
        selector: 'f-execute-query',
        template: `
<button class="btn btn-primary form-control int-test-execute-btn"
    type="button"
    (click)="run()" 
    disabled="{{ isEnabled ? '' : 'disabled' }}"
    >
    <span class="glyphicon glyphicon-play"></span>
</button>
`
    }), 
    __metadata('design:paramtypes', [mirror_change_stream_1.MirrorChangeStream, monitor_service_1.MonitorService, hotkey_service_1.HotkeyService])
], ExecuteQueryComponent);
exports.ExecuteQueryComponent = ExecuteQueryComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const connection_selector_component_1 = require('./connection-selector.component');
const execute_query_component_1 = require('./execute-query.component');
const result_list_component_1 = require('./result-list.component');
const editor_directive_1 = require('../directives/editor.directive');
let QuerySuiteComponent = class QuerySuiteComponent {
};
QuerySuiteComponent = __decorate([
    core_1.Component({
        selector: 'f-query-suite',
        directives: [editor_directive_1.EditorDirective, execute_query_component_1.ExecuteQueryComponent, connection_selector_component_1.ConnectionSelectorComponent, result_list_component_1.ResultListComponent],
        template: `
<div class="container-fluid query-editor-suite">
    <div class="row">
        <div class="col-md-1">
            <p><f-execute-query></f-execute-query></p>
        </div>
        <div class="col-md-11">
            <p><f-connection-selector></f-connection-selector></p>
        </div>
    </div>
    <div class="row">
        <div class="col-md-12">
            <p><textarea editor></textarea></p>
        </div>
    </div>
</div>
<f-result-list></f-result-list>
`
    }), 
    __metadata('design:paramtypes', [])
], QuerySuiteComponent);
exports.QuerySuiteComponent = QuerySuiteComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const query_service_1 = require('../services/query.service');
const query_result_1 = require('../models/query-result');
class ColumnSizing {
}
let ResultDisplayComponent = class ResultDisplayComponent {
    constructor(query) {
        this.query = query;
        this.sizes = [];
        this.availableWidth = null;
        this.dragging = null;
        this.outputWidth = 0;
        this.outputHeight = 0;
        this.outputColumnOffset = 0;
        this.layoutFor = null;
        this.outputOverflowing = false;
    }
    showResult(page) {
        this.query.setActivePage(this.result.id, page.id);
    }
    get activePage() {
        return this.result.pages.find(p => p.active);
    }
    columnWidth(idx, isHeader) {
        if (this.sizes[idx]) {
            let isLast = this.sizes.length - 1 === idx;
            return this.sizes[idx].width + 'px';
        }
        return '0';
    }
    updateColumns() {
        let rowOverflower = document.querySelector('.output-table-rows');
        if (rowOverflower) {
            this.outputColumnOffset = rowOverflower.scrollLeft;
        }
    }
    dragClass() {
        return this.dragging ? 'resizing-columns' : '';
    }
    ngAfterContentChecked() {
        if (this.activePage && this.activePage.id !== this.layoutFor && this.layoutFor !== null) {
            this.sizes = [];
            this.availableWidth = 0;
        }
        let rowOverflower = document.querySelector('.output-table-rows');
        let container = document.querySelector('.result-display-container');
        if (rowOverflower) {
            let oldHeight = this.outputHeight;
            this.outputHeight = container.clientHeight -
                rowOverflower.parentElement.clientHeight;
            let oldWidth = this.availableWidth;
            let newWidth = container.clientWidth;
            let isOverflowing = rowOverflower.clientHeight < rowOverflower.scrollHeight;
            let overflowChanged = isOverflowing !== this.outputOverflowing;
            let sizeChanged = oldHeight !== this.outputHeight ||
                this.availableWidth !== newWidth;
            this.availableWidth = newWidth;
            this.outputOverflowing = isOverflowing;
            if (sizeChanged || overflowChanged) {
                if (this.sizes.length > 0) {
                    this.layoutResize(oldWidth, newWidth, overflowChanged);
                }
                else {
                    this.layoutInitial();
                }
            }
        }
    }
    dragMove(event) {
        if (this.dragging !== null && this.sizes[this.dragging]) {
            let delta = event.clientX - this.dragClientX;
            let oldWidth = this.sizes[this.dragging].width;
            let newWidth = this.sizes[this.dragging].width + delta;
            // set new width with limits
            this.sizes[this.dragging].width = newWidth <= 30 ? 30 : newWidth;
            // check if we changed anything
            let actualDelta = this.sizes[this.dragging].width - oldWidth;
            let performedSizing = actualDelta !== 0;
            let nextSizing = false;
            // adjust the next column, if possible, and only if the primary col isnt yet 30
            if (this.dragging < this.sizes.length - 1 && performedSizing) {
                let endCol = this.sizes[this.dragging + 1];
                // we only adjust if the column wasnt yet < 30, or we're growing the it
                if (endCol.width > 30 || actualDelta < 0) {
                    nextSizing = true;
                    endCol.width += (-1 * actualDelta);
                }
            }
            this.updateTableWidth();
            // ensure we dont shrink under width of window
            if (performedSizing && !nextSizing && this.dragging === this.sizes.length - 1 && actualDelta < 0
                && this.outputWidth < (this.availableWidth - 17)) {
                let currVal = this.sizes[this.sizes.length - 1].width;
                this.sizes[this.sizes.length - 1].width -= actualDelta;
                this.updateTableWidth();
            }
            if (performedSizing) {
                this.dragClientX = event.clientX;
            }
        }
    }
    dragStart(event, idx) {
        this.dragging = idx;
        this.dragClientX = event.clientX;
    }
    dragEnd(event) {
        this.dragging = null;
    }
    layoutResize(from, to, overflowChanged) {
        if (overflowChanged) {
            // chrome scrollbar is 17 pixels. subtract from each non-fixed col
            let subtract = 17;
            let variableCnt = this.sizes.filter(s => !s.fixed).length;
            if (variableCnt > 0) {
                let subPart = subtract;
                this.sizes.forEach(size => {
                    if (!size.fixed && subPart > 0) {
                        let sub = Math.ceil((subtract / variableCnt));
                        size.width -= sub;
                        subPart -= sub;
                    }
                });
                if (subPart !== 0) {
                    console.warn('overflowChanged found remaining bit', subPart);
                    this.sizes.find(size => size.fixed).width += subPart;
                }
            }
        }
        let delta = (to - from) / this.sizes.filter(x => !x.fixed).length;
        this.sizes.forEach(size => {
            if (!size.fixed) {
                size.width += delta;
            }
        });
        this.updateTableWidth();
    }
    // todo fakes font sizes in a crude way
    layoutInitial() {
        let charSize = 13;
        let page = this.activePage;
        if (!page) {
            return;
        }
        else {
            this.layoutFor = page.id;
        }
        if (this.sizes.length === 0) {
            this.sizes = [{
                    fixed: true,
                    width: 0,
                    column: (page.rows.length.toString().length + 1) * charSize,
                    userOffset: 0
                }].concat(page.columnTypes.map((colType, idx) => {
                let name = page.columns[idx];
                let fixed = ['string'].indexOf(colType.toLocaleLowerCase()) === -1;
                return {
                    width: 0,
                    column: name.length * charSize,
                    fixed: fixed,
                    userOffset: 0
                };
            }));
        }
        let fixedWidth = this.sizes.reduce((acc, size) => {
            return acc + (size.fixed ? size.column : 0);
        }, 0);
        let avail = this.availableWidth;
        let availableFlex = (avail - fixedWidth) / (this.sizes.filter(size => !size.fixed).length);
        this.sizes.forEach((size, idx) => {
            if (size.fixed) {
                size.width = size.column;
            }
            else {
                size.width = availableFlex < size.column ? size.column : availableFlex;
            }
        });
        this.updateTableWidth();
    }
    get roundtripTime() {
        let ts = '';
        if (!this.result.loading) {
            let ticks = this.result.finished.getTime() - this.result.created.getTime();
            let unit = ticks < 1000 ? 'ms' : 'sec';
            let div = ticks < 1000 ? 1 : 1000;
            let fixed = ticks < 1000 ? 0 : 2;
            ts = `${(ticks / div).toFixed(fixed)} ${unit}.`;
        }
        return ts;
    }
    updateTableWidth() {
        this.outputWidth = this.sizes.reduce((acc, size) => {
            return acc + size.width;
        }, 0);
    }
    calcHeight() {
        return this.outputHeight ? this.outputHeight + 'px' : '';
    }
};
__decorate([
    core_1.Input(), 
    __metadata('design:type', query_result_1.QueryResult)
], ResultDisplayComponent.prototype, "result", void 0);
ResultDisplayComponent = __decorate([
    core_1.Component({
        selector: 'f-result-display',
        template: `
    <div class="result-display-component {{dragClass()}}" style="width: 100vw"
        (window:mousemove)="dragMove($event)"
        (window:mouseup)="dragEnd($event)">
        <div class="output-table-overview">
            <p class="pull-right">
                <em>{{result.loading ? 'Loading' : ''}}{{roundtripTime}}</em>
            </p>
            <div class="btn-group" role="group">
                <button 
                    *ngFor="let page of result.pages"
                    (click)="showResult(page)"
                    type="button" class="btn btn-default {{page.active ? 'active' : ''}}">
                    {{page.title}} 
                </button>
            </div>
        </div>
        <div *ngIf="activePage" class="output-table-header" [style.width]="outputWidth + 'px'" 
            [style.marginLeft]="(-1 * outputColumnOffset) + 'px'"
            style="overflow:hidden">
            <div [style.width]="columnWidth(0, true)">
                <div title="Row #">&nbsp;</div>
                <div class="output-table-col-dragger"
                    (mousedown)="dragStart($event, 0)"></div>
            </div>
            <div *ngFor="let head of activePage.columns; let colIdx = index"
                [style.width]="columnWidth(colIdx + 1, true)"
                >
                    <div [innerText]="head" [title]="head"></div>
                    <div class="output-table-col-dragger"
                        (mousedown)="dragStart($event, colIdx + 1)"></div>
            </div>
        </div>
        <div *ngIf="activePage" class="output-table-rows" [style.height]="calcHeight()"
            (scroll)="updateColumns()">
            <div *ngFor="let row of activePage.rows; let rowIdx = index" [style.width]="outputWidth + 'px'">
                <div [style.width]="columnWidth(0)" [title]="'Row ' + rowIdx + 1"><div>{{rowIdx + 1}}</div></div>
                <div *ngFor="let cell of row; let colIdx = index"
                    [style.width]="columnWidth(colIdx + 1)"
                    ><div [innerText]="cell" [title]="cell"></div></div>
            </div>
        </div>
    </div>
`
    }), 
    __metadata('design:paramtypes', [query_service_1.QueryService])
], ResultDisplayComponent);
exports.ResultDisplayComponent = ResultDisplayComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const query_service_1 = require('../services/query.service');
const tab_service_1 = require('../services/tab.service');
const result_display_component_1 = require('./result-display.component');
class ResultMap {
}
let ResultListComponent = class ResultListComponent {
    constructor(query, tabs) {
        this.currentResults = [];
        this.offset = 84;
        tabs.active
            .combineLatest(query.activeResult, (tab, store) => {
            let results = store.tab(tab.id);
            return {
                tab: tab,
                results: results
            };
        })
            .subscribe((x) => {
            this.currentResults = x.results;
        });
    }
    editorOffset() {
        return (this.offset + 64 /* heder */) + 'px';
    }
    ngAfterContentChecked() {
        this.offset = document.querySelector('.query-editor-suite').clientHeight;
    }
};
ResultListComponent = __decorate([
    core_1.Component({
        selector: 'f-result-list',
        directives: [result_display_component_1.ResultDisplayComponent],
        template: `
<div class="result-display-container" *ngIf="currentResults.length > 0" [style.top]="editorOffset()">
    <f-result-display [result]="currentResults[0]"></f-result-display>
</div>
`
    }), 
    __metadata('design:paramtypes', [query_service_1.QueryService, tab_service_1.TabService])
], ResultListComponent);
exports.ResultListComponent = ResultListComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const overlay_service_1 = require('../services/overlay.service');
const connection_service_1 = require('../services/connection.service');
let StartPageComponent = class StartPageComponent {
    constructor(overlay, connectionService) {
        this.overlay = overlay;
        this.connectionService = connectionService;
    }
    connectionsToggle() {
        this.overlay.showConnections();
    }
};
StartPageComponent = __decorate([
    core_1.Component({
        selector: 'f-start-page',
        template: `
<div class="container-fluid int-test-start-page">
    <div class="row">
        <div *ngIf="!connectionService.defaultConnection">
            <div class="col-md-12">
                <p>
                    Start by adding a database connection 
                    using the Connection Manager. <em>(shortcut: ctrl + d)</em>
                </p>
                <p>
                    <button (click)="connectionsToggle()" class="btn btn-default">Open Connection Manager</button>
                </p>
            </div>
        </div>
    </div>
</div>
`
    }), 
    __metadata('design:paramtypes', [overlay_service_1.OverlayService, connection_service_1.ConnectionService])
], StartPageComponent);
exports.StartPageComponent = StartPageComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const router_deprecated_1 = require('@angular/router-deprecated');
const tab_service_1 = require('../services/tab.service');
const ng2_bootstrap_1 = require('ng2-bootstrap/ng2-bootstrap');
let TabListComponent = class TabListComponent {
    constructor(tabs) {
        this.tabs = tabs;
        this.title = '';
        this.currentTabs = [];
        this.tabsEnabled = false;
        tabs.tabs
            .subscribe(ts => {
            this.currentTabs = ts;
            this.tabsEnabled = ts.length > 0;
        });
    }
    newTab() {
        this.tabs.newTab();
    }
    goto(id) {
        this.tabs.goto(id);
    }
    get viewTitle() {
        return this.tabsEnabled ? '' : 'Hello!';
    }
    get viewTitleClass() {
        return this.tabsEnabled ? 'hidden' : '';
    }
};
TabListComponent = __decorate([
    core_1.Component({
        selector: 'f-tab-list',
        directives: [router_deprecated_1.ROUTER_DIRECTIVES, ng2_bootstrap_1.DROPDOWN_DIRECTIVES],
        template: `
<nav class="navbar navbar-default navbar-fixed-top">
    <div class="container-fluid int-test-tab-list">
        <div class="navbar-header {{viewTitleClass}}">
            <a class="navbar-brand">{{viewTitle}}</a>
        </div>
        <div class="navbar-collapse collapse" *ngIf="tabsEnabled">
            <ul class="nav navbar-nav">
                <li *ngFor="let tab of currentTabs"
                     class="{{tab.active ? 'active' : ''}}">
                    <a (click)="goto(tab.id)" href="javascript:void 0">
                        {{tab.title}}
                    </a>
                </li>
                <li>
                    <a (click)="newTab()" href="javascript:void 0"><span class="glyphicon glyphicon-plus"></span></a>
                </li>
            </ul>
        </div>
    </div>
</nav>
`
    }), 
    __metadata('design:paramtypes', [tab_service_1.TabService])
], TabListComponent);
exports.TabListComponent = TabListComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const tab_service_1 = require('../services/tab.service');
const query_suite_component_1 = require('./query-suite.component');
const start_page_component_1 = require('./start-page.component');
let TabViewComponent = class TabViewComponent {
    constructor(tabs) {
        this.tabs = tabs;
        tabs.hasTabs
            .subscribe(hasTabs => {
            this.startVisible = !hasTabs;
            this.editorVisible = hasTabs;
        });
    }
};
TabViewComponent = __decorate([
    core_1.Component({
        selector: 'f-tab-view',
        directives: [query_suite_component_1.QuerySuiteComponent, start_page_component_1.StartPageComponent],
        template: `
<div *ngIf="editorVisible" class="my-editor">
    <f-query-suite></f-query-suite>
</div>
<div *ngIf="startVisible" class="my-start">
    <f-start-page></f-start-page>
</div>
`
    }), 
    __metadata('design:paramtypes', [tab_service_1.TabService])
], TabViewComponent);
exports.TabViewComponent = TabViewComponent;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const editor_service_1 = require('../services/editor.service');
const tab_service_1 = require('../services/tab.service');
const omnisharp_service_1 = require('../services/omnisharp.service');
const mirror_change_stream_1 = require('../services/mirror-change.stream');
const CodeMirror = require('codemirror');
require('codemirror/addon/hint/show-hint');
require('codemirror/mode/clike/clike');
require('codemirror/addon/lint/lint');
let onetimeBullshit = false;
CodeMirror.commands.autocomplete = function (cm) {
    cm.showHint({ hint: CodeMirror.hint.ajax });
};
const mac = CodeMirror.keyMap.default === CodeMirror.keyMap.macDefault;
CodeMirror.keyMap.default[(mac ? 'Cmd' : 'Ctrl') + '-Space'] = 'autocomplete';
let omnisharpResolver = null;
const omnisharpInject = new Promise((res) => {
    omnisharpResolver = res;
});
CodeMirror.registerHelper('lint', 'text/x-csharp', (text, callback) => {
    omnisharpInject.then(svc => {
        let endLine = text.split(/$/g).length - 1;
        let endColumn = endLine >= 0 ? text.split(/$/g)[endLine].length : text.length;
        endLine = endLine < 0 ? 0 : endLine;
        svc.lintRequests.next({ callback: callback, endLine: endLine, endColumn: endColumn });
    });
});
CodeMirror.lint['text/x-csharp'].async = true;
CodeMirror.registerHelper('hint', 'ajax', (mirror, callback) => {
    // todo: test if syntax mode changes anything,
    // otherwise findWordAt seems pretty useless, 
    // returning random whitespace/syntax as words.
    // for now this manual parsing, that doesn't work cross lines
    const memberAccessTest = /\.$/;
    const partialMemberAccessTest = /\.(\w*)$/; // this cant work
    // const tab = this.tabService.get(mirror._tab);
    const cur = mirror.getCursor();
    const editorLine = mirror.getRange({ line: cur.line, ch: 0 }, cur);
    let fragment = null;
    let range = {
        head: { line: cur.line, ch: cur.ch },
        anchor: { line: cur.line, ch: cur.ch }
    };
    if (!memberAccessTest.test(editorLine)) {
        let match = editorLine.match(partialMemberAccessTest);
        if (match[1] && match[1].length > 0) {
            fragment = match[1];
            range.anchor.ch = match.index + 1;
        }
    }
    let request = {
        column: cur.ch,
        line: cur.line,
        wantKind: true,
        wantDocumentationForEveryCompletionResult: true,
        wordToComplete: fragment,
        wantReturnType: true,
        wantMethodHeader: true
    };
    omnisharpInject
        .then(svc => {
        svc.autocomplete(request)
            .subscribe(list => {
            callback({
                list: list,
                from: range.anchor,
                to: range.head
            });
        });
    });
});
CodeMirror.hint.ajax.async = true;
let EditorDirective = class EditorDirective {
    constructor(editorService, omnisharpService, tabService, mirrorChangeStream, element, renderer) {
        this.editorService = editorService;
        this.omnisharpService = omnisharpService;
        this.tabService = tabService;
        this.mirrorChangeStream = mirrorChangeStream;
        this.element = element;
        this.renderer = renderer;
        this.current = null;
        this.textContent = null;
        this.touched = false;
        this.editor = CodeMirror.fromTextArea(element.nativeElement, this.editorOptions());
        mirrorChangeStream.initMirror(this.editor);
        // todo one time service injection hack
        if (omnisharpResolver) {
            omnisharpResolver(omnisharpService);
            omnisharpResolver = null;
        }
    }
    codemirrorValueChanged(mirror) {
        this.touched = true;
        let newValue = mirror.getValue();
        let cur = mirror.getCursor();
        let range = mirror.findWordAt(cur);
        let fragment = mirror.getRange(range.anchor, range.head);
        if (fragment === '.' || fragment === ').') {
            CodeMirror.commands.autocomplete(mirror);
        }
        this.editorService.set(this.current, newValue);
    }
    ngOnInit() {
        this.editor.refresh();
    }
    editorOptions() {
        return {
            lineNumbers: true,
            gutters: ['CodeMirror-lint-markers'],
            lint: true,
            smartIndent: false,
            matchBrackets: true,
            viewportMargin: Infinity,
            showCursorWhenSelecting: true,
            mode: 'text/x-csharp'
        };
    }
};
EditorDirective = __decorate([
    core_1.Directive({
        selector: '[editor]'
    }), 
    __metadata('design:paramtypes', [editor_service_1.EditorService, omnisharp_service_1.OmnisharpService, tab_service_1.TabService, mirror_change_stream_1.MirrorChangeStream, core_1.ElementRef, core_1.Renderer])
], EditorDirective);
exports.EditorDirective = EditorDirective;

"use strict";
// input for omnisharp
class AutocompletionQuery {
}
exports.AutocompletionQuery = AutocompletionQuery;

"use strict";
class AutocompletionResult {
}
exports.AutocompletionResult = AutocompletionResult;

"use strict";
class CodeCheckResult {
}
exports.CodeCheckResult = CodeCheckResult;

"use strict";
class CompletionItem {
}
exports.CompletionItem = CompletionItem;

"use strict";
class Connection {
    constructor(connectionString) {
        this.editing = false;
        this.connectionString = null;
        this.connectionString = connectionString;
    }
    toJSON() {
        return {
            id: this.id,
            connectionString: this.connectionString
        };
    }
}
exports.Connection = Connection;

"use strict";
class EditorChange {
}
exports.EditorChange = EditorChange;

"use strict";
class QueryRequest {
}
exports.QueryRequest = QueryRequest;

"use strict";
class QueryResult {
    constructor() {
        this.pages = [];
    }
}
exports.QueryResult = QueryResult;

"use strict";
class ResultPage {
}
exports.ResultPage = ResultPage;

"use strict";
const _ = require('lodash');
class ResultStore {
    tab(id) {
        let l = this.data.get(id);
        return _.chain(l).sortBy(x => x.created).reverse().value();
    }
    setActive(queryResultId, pageId) {
        let tabId = null; // todo: pass from component instead
        let newList = null;
        for (let [key, queries] of this.data) {
            let query = queries.find(q => q.id === queryResultId);
            if (query) {
                newList = queries;
                tabId = key;
                break;
            }
        }
        this.data.set(tabId, newList.map(qr => {
            if (qr.id === queryResultId) {
                qr.pages = qr.pages.map(p => {
                    const match = p.id === pageId;
                    // if we mark the current active, it will invert
                    p.active = match && p.active ? false : match;
                    return p;
                });
            }
            return qr;
        }));
        return this;
    }
    addLoading(tabId) {
        if (!this.data) {
            this.data = new Map();
        }
        if (!this.data.has(tabId)) {
            this.data.set(tabId, []);
        }
        this.data.set(tabId, [{
                tabId: tabId,
                loading: true,
                pages: [],
                created: new Date()
            }, ...this.data.get(tabId)]);
        return this;
    }
    add(tabId, result) {
        let all = this.data.get(tabId);
        // todo multiple loadings in same tab ...
        let loading = all.find(x => x.loading && x.tabId === tabId);
        let filtered = all.filter(x => !(x.loading && x.tabId === tabId));
        Assert(all.length === filtered.length + 1, `Length mismatch: ${all.length} === ${filtered.length + 1}`);
        this.data.set(tabId, [{
                id: result.id,
                tabId: result.tabId,
                query: result.query,
                connectionString: result.connectionString,
                created: loading.created,
                finished: result.finished,
                loading: false,
                pages: result.pages
            }, ...filtered]);
        return this;
    }
}
exports.ResultStore = ResultStore;

"use strict";
class Tab {
}
exports.Tab = Tab;

"use strict";
class TemplateResult {
}
exports.TemplateResult = TemplateResult;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const Rx_1 = require('rxjs/Rx');
let ConnectionService = class ConnectionService {
    constructor() {
        this.key = 'connections';
        this.ops = new Rx_1.ReplaySubject();
        // initial values
        this.ops.next(x => x);
        this.stream = this.ops
            .scan((acc, op) => {
            return op([...acc]);
        }, this.load());
        this.stream.subscribe(cs => {
            this.save(cs);
        });
    }
    get all() {
        return this.stream;
    }
    add(conn) {
        this.ops.next((conns) => {
            conn.id = this.getNextValidId(0, conns);
            return [conn, ...conns];
        });
    }
    remove(conn) {
        this.ops.next((conns) => {
            return [...conns.filter(x => x.id !== conn.id)];
        });
    }
    update(conn) {
        this.ops.next((conns) => {
            let idx = -1;
            conns.forEach((x, i) => {
                if (x.id === conn.id) {
                    idx = i;
                }
            });
            Assert(idx >= 0, 'update id not found');
            return [...conns.slice(0, idx),
                {
                    id: conn.id,
                    connectionString: conn.connectionString,
                }, ...conns.slice(idx + 1)];
        });
    }
    load() {
        try {
            let raw = localStorage.getItem(this.key);
            return raw ? JSON.parse(raw) : [];
        }
        catch (e) {
            console.error(`connection-service load exception: ${e}`);
            return [];
        }
    }
    save(conns) {
        localStorage.setItem(this.key, JSON.stringify(conns));
    }
    getNextValidId(guess = 0, conns) {
        if (conns.find(c => c.id === guess)) {
            return this.getNextValidId(guess + 1, conns);
        }
        return guess;
    }
};
ConnectionService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [])
], ConnectionService);
exports.ConnectionService = ConnectionService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const editor_change_1 = require('../models/editor-change');
const tab_service_1 = require('./tab.service');
const Rx_1 = require('rxjs/Rx');
let EditorService = class EditorService {
    constructor(tabs) {
        this.tabs = tabs;
        this.buffers = {};
        this.changes = new Rx_1.Subject();
    }
    get(tab) {
        return this.buffers[tab.id] || '\n\n';
    }
    set(tab, text, isDirective = true) {
        let change = new editor_change_1.EditorChange();
        change.newText = text;
        change.tabId = tab.id;
        if (!isDirective) {
            this.changes.next(change);
        }
        this.buffers[tab.id] = text;
    }
    errors(tabId) {
        return new Rx_1.Observable(obs => {
            setTimeout(function () {
                obs.next({
                    messages: []
                });
            }, 10000);
        });
    }
};
EditorService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [tab_service_1.TabService])
], EditorService);
exports.EditorService = EditorService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const Rx_1 = require('rxjs/Rx');
const ipc = electronRequire('electron').ipcRenderer;
let HotkeyService = class HotkeyService {
    constructor(ngZone) {
        this.connMan = new Rx_1.Subject();
        this.exeQuery = new Rx_1.Subject();
        this.executeQuery = this.exeQuery
            .asObservable();
        this.connectionManager = this.connMan
            .asObservable();
        ipc.on('application-event', (event, msg) => {
            ngZone.run(() => {
                if (msg === 'connections-panel') {
                    this.connMan.next(true);
                }
                else if (msg === 'execute-query') {
                    this.exeQuery.next(true);
                }
            });
        });
    }
};
HotkeyService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [core_1.NgZone])
], HotkeyService);
exports.HotkeyService = HotkeyService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const ipc = electronRequire('electron').ipcRenderer;
const fs = electronRequire('fs');
const path = electronRequire('path');
const isProduction = MODE !== 'DEVELOPMENT';
const dirname = isProduction ? path.dirname(process.resourcesPath) : path.dirname(path.dirname(__dirname));
const logFile = path.normalize(`${dirname}/linq-editor${isProduction ? '' : '.dev'}.log`);
let LogService = class LogService {
    constructor() {
        this.debug = MODE === 'DEVELOPMENT';
        this.logs = [];
        ipc.on('application-event', this.applicationEventHandler.bind(this));
        this.log('log.service', `debug=${this.debug}`);
    }
    log(stamp, msg) {
        const str = `${stamp} => \n${msg}`;
        if (this.debug) {
            console.log(str);
        }
        this.logs.push(str);
    }
    applicationEventHandler(event, msg) {
        if (msg === 'close') {
            // allow shit to flush to the log
            setTimeout(() => {
                const logOut = this.logs.join('\n\n- LOG --------------------------------------------------------------\n\n');
                fs.writeFile(logFile, logOut, function (err) {
                    if (err) {
                        console.error(`writeFile.err ${err}`);
                    }
                    ipc.send('application-event', 'close-log');
                });
            }, 2000);
        }
    }
};
LogService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [])
], LogService);
exports.LogService = LogService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const tab_service_1 = require('../services/tab.service');
const Rx_1 = require('rxjs/Rx');
class BufferMap {
}
class BufferValue {
}
let THERE_CAN_BE_ONLY_ONE = false;
let MirrorChangeStream = class MirrorChangeStream {
    constructor(tabs) {
        this.tabs = tabs;
        this.sub = new Rx_1.Subject();
        this.bufferOps = new Rx_1.ReplaySubject();
        this.currentOps = new Rx_1.ReplaySubject();
        this.mirror = null;
        this.buffers = this.bufferOps
            .scan((bfs, op) => {
            let res = op(bfs);
            return res;
        }, { tabs: [] })
            .publishReplay()
            .refCount();
        this.executing = this.currentOps
            .scan((str, op) => {
            return op(str);
        }, '')
            .publishReplay()
            .refCount();
        this.buffers.subscribe();
    }
    execute() {
        this.currentOps.next((str) => {
            return this.mirror.getDoc().getValue();
        });
    }
    initMirror(mirror) {
        Assert(this.mirror === null, 'mirror was set');
        this.mirror = mirror;
        this.tabs.activeBase
            .subscribe(tabs => {
            this.bufferOps.next((m) => {
                let toTab = tabs[0];
                Assert(toTab, 'no active tab');
                let buffers = m.tabs;
                let bufferFilter = x => x.tabId === toTab.id && x.connId === toTab.connectionId;
                if (!buffers.find(bufferFilter)) {
                    // if the target buffer doesn't exist, create it
                    buffers = [
                        {
                            tabId: toTab.id,
                            connId: toTab.connectionId,
                            active: false,
                            value: ''
                        },
                        ...buffers
                    ];
                }
                let fromBuffer = buffers.find(x => x.active);
                let toBuffer = buffers.find(bufferFilter);
                // highlander is set further down, the first time here, it'll be false
                let sameTab = fromBuffer && toTab.id === fromBuffer.tabId || !THERE_CAN_BE_ONLY_ONE;
                let there_was_one = false;
                Assert(toBuffer, `no toBuffer found`);
                if (fromBuffer) {
                    // update the value of the fromBuffer, if it existed
                    buffers = buffers.map(x => {
                        if (x.tabId === fromBuffer.tabId && x.connId === fromBuffer.connId) {
                            x.value = this.mirror.getDoc().getValue();
                        }
                        return x;
                    });
                    fromBuffer = buffers.find(x => x.active); // update
                }
                else {
                    // fromBuffer should only be missing for the very first tab created
                    Assert(!THERE_CAN_BE_ONLY_ONE, 'ZOMG');
                    there_was_one = THERE_CAN_BE_ONLY_ONE = true;
                }
                // set active
                buffers = buffers.map(x => {
                    x.active = x.tabId === toTab.id && x.connId === toTab.connectionId;
                    return x;
                });
                // branch, depending on what we think happened.
                if (!sameTab) {
                    Assert(toBuffer, `not same tab, but no toBuffer ${toBuffer}`);
                    // switched to different tab, set mirror text to restored value
                    mirror.getDoc().setValue(toBuffer.value);
                }
                else if (!there_was_one) {
                    Assert(toBuffer && fromBuffer, 'same tab, but no fromBuffer');
                    // flush an update to omnisharp, accounts for how
                    // the buffer was modified while we were using 
                    // another connection context.
                    let lines = toBuffer ? toBuffer.value.split('\n') : [];
                    let endLineOffset = toBuffer ? lines.length - 1 : 0;
                    let endColumnOffset = toBuffer ? lines[lines.length - 1].length : 0;
                    this.sub.next({
                        newText: fromBuffer.value,
                        origin: 'totally-not-fake',
                        created: performance.now(),
                        startColumn: 0,
                        startLine: 0,
                        endColumn: 0 + endColumnOffset,
                        endLine: 0 + endLineOffset
                    });
                }
                return {
                    tabs: buffers
                };
            });
        });
        mirror.on('change', (_, cs) => {
            if (cs.origin !== 'setValue') {
                this.sub.next(this.mapEvent(cs));
            }
        });
    }
    get changes() {
        return this.sub
            .asObservable();
    }
    mapEvent(val) {
        return {
            startLine: val.from.line,
            startColumn: val.from.ch,
            endLine: val.to.line,
            endColumn: val.to.ch,
            newText: val.text.join('\n'),
            origin: val.origin,
            created: performance.now()
        };
    }
};
MirrorChangeStream = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [tab_service_1.TabService])
], MirrorChangeStream);
exports.MirrorChangeStream = MirrorChangeStream;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const http_1 = require('@angular/http');
const log_service_1 = require('./log.service');
const config_1 = require('../config');
// todo maybe make global?
const child_process = electronRequire('child_process');
const ipc = electronRequire('electron').ipcRenderer;
const path = electronRequire('path');
const fs = electronRequire('fs');
const isProduction = MODE !== 'DEVELOPMENT';
// __dirname doesn't seem to work in bundle mode
const dirname = (isProduction ? path.normalize(process.resourcesPath + '/app') : path.dirname(path.dirname(__dirname)))
    .replace(/%20/g, ' '); // otherwise file fetch fails on windows
let MonitorService = class MonitorService {
    constructor(http, logService) {
        this.http = http;
        this.logService = logService;
        ipc.on('application-event', this.applicationEventHandler.bind(this));
        this.omnisharpReady = new Promise((res, err) => {
            this.omnisharpResolver = (arg) => {
                // some buffer time
                setTimeout(() => { res(arg); }, 500);
            };
        });
        this.queryReady = new Promise((res, err) => {
            this.queryResolver = res;
        });
        this.ready = new Promise((res) => {
            this.omnisharpReady.then(() => {
                this.queryReady.then(() => {
                    res(true);
                });
            });
        });
        this.start();
    }
    applicationEventHandler(event, msg) {
        if (msg === 'close') {
            let queryCb = () => ipc.send('application-event', 'close-query-engine');
            let omniCb = () => ipc.send('application-event', 'close-omnisharp');
            this.http.get(this.action(config_1.default.omnisharpPort, 'stopserver')).subscribe(omniCb, omniCb);
            this.http.get(this.action(config_1.default.queryEnginePort, 'stopserver')).subscribe(queryCb, queryCb);
        }
    }
    start() {
        let queryParams = this.queryCmd();
        let omnisharpCmd = this.omnisharpCmd().cmd;
        this.logService.log('monitor.service', `starting queryengine: ${JSON.stringify(queryParams)}`);
        this.logService.log('monitor.service', `starting omnisharp: ${omnisharpCmd}`);
        this.http.get(this.action(config_1.default.omnisharpPort, 'checkreadystatus'))
            .subscribe(ok => {
            this.omnisharpResolver(true);
        }, error => {
            this.startProcess(omnisharpCmd, {});
            this.checkBackends(config_1.default.omnisharpPort);
        });
        this.http.get(this.action(config_1.default.queryEnginePort, 'checkreadystatus'))
            .subscribe(ok => {
            this.queryResolver(true);
        }, error => {
            this.startProcess(queryParams.cmd, { cwd: queryParams.dir });
            this.checkBackends(config_1.default.queryEnginePort);
        });
    }
    checkBackends(port) {
        this.http.get(this.action(port, 'checkreadystatus'))
            .subscribe(ok => {
            if (port === config_1.default.omnisharpPort) {
                this.omnisharpResolver(true);
            }
            if (port === config_1.default.queryEnginePort) {
                this.queryResolver(true);
            }
        }, error => {
            let lbl = port === config_1.default.omnisharpPort ? 'omnisharp' : 'query-engine';
            setTimeout(() => { this.checkBackends(port); }, 500);
        });
    }
    startProcess(cmd, options) {
        child_process.exec(cmd, options, (error, stdout, stderr) => {
            this.logService.log('stdout', stdout);
            this.logService.log('stderr', stderr);
            if (error !== null) {
                this.logService.log('error', error);
            }
        });
    }
    action(port, name) {
        return `http://localhost:${port}/${name}`;
    }
    queryCmd() {
        let dir = isProduction ? `${dirname}/query` : dirname;
        let cmd = isProduction ? `"${dir}/linq-editor${!IS_LINUX ? '.exe' : ''}"` :
            `"${config_1.default.dotnetDebugPath}" run `;
        return { dir: dir, cmd: cmd };
    }
    omnisharpCmd() {
        let exePath = `"${dirname}/omnisharp/OmniSharp${!IS_LINUX ? '.exe' : ''}"`;
        console.log('config.omnisharpProjectPath', config_1.default.omnisharpProjectPath);
        let slnPath = IS_LINUX ? config_1.default.omnisharpProjectPath.replace(/\\/g, '/')
            : config_1.default.omnisharpProjectPath.replace(/\//g, '\\');
        let cmd = `${exePath} -s ${slnPath} -p ${config_1.default.omnisharpPort}`;
        return { dir: dirname, cmd: cmd };
    }
};
MonitorService = __decorate([
    // otherwise file fetch fails on windows
    core_1.Injectable(), 
    __metadata('design:paramtypes', [http_1.Http, log_service_1.LogService])
], MonitorService);
exports.MonitorService = MonitorService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const http_1 = require('@angular/http');
const Rx_1 = require('rxjs/Rx');
require('rxjs/Rx');
const _ = require('lodash');
const CodeMirror = require('codemirror');
const uuid = require('node-uuid');
const tab_service_1 = require('../services/tab.service');
const monitor_service_1 = require('../services/monitor.service');
const mirror_change_stream_1 = require('../services/mirror-change.stream');
const connection_service_1 = require('../services/connection.service');
const config_1 = require('../config');
const path = electronRequire('path');
class SessionMap {
}
class UpdateMap {
}
class QueryTemplateResult {
}
class CodeCheckMap {
}
class LintRequestMap {
}
let OmnisharpService = class OmnisharpService {
    constructor(conns, monitorService, mirrorChangeStream, tabs, http) {
        this.conns = conns;
        this.monitorService = monitorService;
        this.mirrorChangeStream = mirrorChangeStream;
        this.tabs = tabs;
        this.http = http;
        this.port = config_1.default.omnisharpPort;
        this.initialized = {};
        this.lintRequests = new Rx_1.Subject();
        const startUpPauser = Rx_1.Observable.fromPromise(monitorService.ready)
            .startWith(false);
        this.sessions = startUpPauser
            .switchMap(ready => {
            return ready ? tabs.newContext : Rx_1.Observable.never();
        })
            .withLatestFrom(conns.all, (tab, newConns) => { return { tab: tab, conns: newConns }; })
            .scan((ctx, newCtx) => {
            const conn = newCtx.conns.find(x => x.id === newCtx.tab.connectionId);
            const tabId = newCtx.tab.id;
            return { conn: conn, tabId: tabId };
        }, {})
            .flatMap((x) => {
            let req = { connectionString: x.conn.connectionString, text: '' };
            return new Rx_1.Observable((obs) => {
                http.post('http://localhost:8111/querytemplate', JSON.stringify(req))
                    .map(res => res.json())
                    .subscribe(data => {
                    obs.next({
                        tabId: x.tabId,
                        buffer: data.Template,
                        connId: x.conn.id,
                        templateOffset: data.LineOffset
                    });
                    obs.complete();
                });
            });
        })
            .flatMap(x => {
            let json = {
                FileName: `${config_1.default.omnisharpProjectPath}/b${uuid.v4().replace(/\-/g, '')}.cs`,
                FromDisk: false,
                Buffer: x.buffer,
            };
            return new Rx_1.Observable((obs) => {
                http.post('http://localhost:2000/updatebuffer', JSON.stringify(json))
                    .map(res => res.json)
                    .subscribe(data => {
                    obs.next({
                        fileName: json.FileName,
                        tabId: x.tabId,
                        connId: x.connId,
                        templateOffset: x.templateOffset
                    });
                    obs.complete();
                });
            });
        })
            .startWith(null)
            .scan((sessions, newSession) => {
            if (newSession) {
                return [newSession, ...sessions];
            }
            return sessions;
        }, [])
            .publishReplay()
            .refCount();
        this.ready = tabs.active
            .combineLatest(this.sessions, (tab, sessions) => {
            let session = sessions.find(x => x.tabId === tab.id && x.connId === tab.connectionId);
            return session !== undefined;
        })
            .distinctUntilChanged();
        this.fileNameAndTemplateInfo = tabs.active
            .combineLatest(this.sessions, (tab, sessions) => {
            let session = sessions.find(x => x.tabId === tab.id && x.connId === tab.connectionId);
            return session !== undefined && { fileName: session.fileName, offset: session.templateOffset };
        })
            .filter(x => !!x) // if session wasn't found, filter undefined/false
        ;
        let mirrorBuffer = Rx_1.Observable.timer(0, 250)
            .combineLatest(this.ready, (val, status) => {
            return status; // ? 42 : status;
        })
            .filter(x => x);
        this.readyState = mirrorChangeStream.changes
            .buffer(mirrorBuffer)
            .filter(x => x.length > 0)
            .withLatestFrom(this.fileNameAndTemplateInfo, (changes, fileNameAndOffset) => {
            return changes.map(x => {
                x.fileName = fileNameAndOffset.fileName;
                x.lineOffset = fileNameAndOffset.offset;
                return x;
            });
        })
            .flatMap(changes => {
            let json = {
                FileName: changes[0].fileName,
                FromDisk: false,
                Changes: changes.map(c => {
                    return {
                        newText: c.newText,
                        startLine: c.startLine + c.lineOffset,
                        startColumn: c.startColumn + 1,
                        endLine: c.endLine + c.lineOffset,
                        endColumn: c.endColumn + 1
                    };
                }),
            };
            return new Rx_1.Observable((obs) => {
                http.post('http://localhost:2000/updatebuffer', JSON.stringify(json))
                    .map(res => res.status === 200)
                    .subscribe(data => {
                    obs.next(performance.now());
                    obs.complete();
                });
            });
        })
            .combineLatest(mirrorChangeStream.changes, (latest, change) => {
            return {
                status: (change.created - latest) <= 0,
                fileName: change.fileName
            };
        })
            .publishReplay()
            .refCount();
        // todo fix this bullshit
        this.readyState2 = new Rx_1.ReplaySubject(1);
        this.readyState
            .subscribe(x => this.readyState2.next(x));
        this.codecheck = this.lintRequests
            .debounceTime(500)
            .withLatestFrom(this.readyState2, (lintReq, status) => {
            return {
                request: lintReq,
                fileName: status.fileName,
            };
        })
            .flatMap(reqAndFilename => {
            return new Rx_1.Observable((obs) => {
                let mapQ = this.mapQuickFixes.bind(this);
                http.post('http://localhost:2000/codecheck', JSON.stringify({ FileName: reqAndFilename.fileName }))
                    .map(res => res.json())
                    .map(mapQ)
                    .subscribe(data => {
                    obs.next({
                        fileName: reqAndFilename.fileName,
                        request: reqAndFilename.request,
                        fixes: this.filterCodeChecks(data)
                    });
                    obs.complete();
                });
            });
        })
            .combineLatest(this.sessions, (codecheck, sessions) => {
            let names = sessions.filter(ss => ss.fileName === codecheck.fileName);
            Assert(names.length < 1, 'session did not contain expected filename');
            Assert(names.length > 1, 'session contained too many expected filenames');
            codecheck.lineOffset = sessions
                .find(ss => ss.fileName === codecheck.fileName)
                .templateOffset;
            return codecheck;
        });
        this.codecheck.subscribe(codecheck => {
            Assert(codecheck.lineOffset !== null && codecheck.lineOffset !== undefined);
            const maxEndl = codecheck.request.endLine;
            const maxEndc = codecheck.request.endColumn;
            let mapped = codecheck.fixes.map(check => {
                let severity = 'error';
                let fromLine = check.line - codecheck.lineOffset;
                let toLine = check.endLine - codecheck.lineOffset;
                let adjustedFrom = fromLine > maxEndl ? maxEndl : fromLine;
                let adjustedFromCol = fromLine > maxEndl ? maxEndc : check.column - 1;
                let adjustedTo = toLine > maxEndl ? maxEndl : toLine;
                let adjustedToCol = toLine > maxEndl ? maxEndc : check.endColumn - 1;
                // to make codemirror work, the location must be inside the text
                if (adjustedFromCol === adjustedToCol && adjustedFrom === adjustedTo && adjustedFromCol > 0) {
                    adjustedFromCol -= 1;
                    // in compensation, we use this css class, to shift the actual DOM marker instead
                    severity = 'eol-error';
                }
                return {
                    from: CodeMirror.Pos(adjustedFrom, adjustedFromCol),
                    to: CodeMirror.Pos(adjustedTo, adjustedToCol),
                    message: check.text,
                    severity: severity
                };
            });
            codecheck.request.callback(mapped);
        });
    }
    autocomplete(request) {
        let mapCodeMirror = this.mapToCodeMirror.bind(this);
        return this.readyState2
            .filter(x => x.status)
            .take(1)
            .withLatestFrom(this.fileNameAndTemplateInfo, (status, fileName) => {
            return { fileName: fileName.fileName, lineOffset: fileName.offset };
        })
            .flatMap(fileInfo => {
            let json = request;
            json.fileName = fileInfo.fileName;
            json.column += 1;
            json.line += fileInfo.lineOffset;
            return new Rx_1.Observable((obs) => {
                this.http.post('http://localhost:2000/autocomplete', JSON.stringify(json))
                    .map(res => res.json())
                    .map(mapCodeMirror)
                    .subscribe(data => {
                    obs.next(data);
                    obs.complete();
                });
            });
        });
    }
    mapToCodeMirror(result) {
        return _.chain(result)
            .groupBy(r => r.CompletionText)
            .map(nameList => {
            // filter out duplicates of the same name and type
            let byType = _.reduce(nameList, (saved, item) => {
                let isCustomDump = item.Description === 'QueryEngine.Inlined.Dumper';
                if (!isCustomDump && !saved.find(s => s.Kind === item.Kind)) {
                    saved.push(item);
                }
                return saved;
            }, []);
            let res = _.sortBy(byType, 'Kind');
            return res;
        })
            .flatten()
            .map((i) => {
            return {
                sortKey: i.CompletionText.toLocaleLowerCase(),
                text: i.CompletionText,
                className: `prop-kind-${i.Kind.toLocaleLowerCase()}`
            };
        })
            .sortBy('sortKey')
            .value();
    }
    mapQuickFixes(result) {
        let fixes = result.QuickFixes;
        return fixes.map(x => {
            return {
                text: this.cleanupMessage(x.Text),
                logLevel: x.LogLevel,
                fileName: x.FileName,
                line: x.Line,
                endLine: x.EndLine,
                column: x.Column,
                endColumn: x.EndColumn
            };
        });
    }
    cleanupMessage(text) {
        let fluf = [
            / \(are you missing a using directive or an assembly reference\?\)/
        ];
        let s = text;
        fluf.forEach(r => {
            s = s.replace(r, '');
        });
        return s;
    }
    filterCodeChecks(checks) {
        let filt = checks
            .filter(c => {
            const isMissingSemicolon = c.text === '; expected';
            const isHidden = c.logLevel === 'Hidden';
            return !(isHidden || isMissingSemicolon);
        });
        return filt;
    }
    action(name) {
        return `http://localhost:${this.port}/${name}`;
    }
};
OmnisharpService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [connection_service_1.ConnectionService, monitor_service_1.MonitorService, mirror_change_stream_1.MirrorChangeStream, tab_service_1.TabService, http_1.Http])
], OmnisharpService);
exports.OmnisharpService = OmnisharpService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const Rx_1 = require('rxjs/Rx');
const hotkey_service_1 = require('./hotkey.service');
let OverlayService = class OverlayService {
    constructor(ngZone, hotkeys) {
        this.ngZone = ngZone;
        this.connSub = new Rx_1.ReplaySubject(1);
        this.connSub.next(false); // set initial value
        hotkeys.connectionManager.subscribe(() => {
            this.toggleConnections();
        });
    }
    showConnections() {
        this.connSub.next(true);
    }
    hideConnections() {
        this.connSub.next(false);
    }
    toggleConnections() {
        this.connections
            .take(1)
            .subscribe(x => x ? this.hideConnections() : this.showConnections());
    }
    get connections() {
        return this.connSub
            .asObservable();
    }
};
OverlayService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [core_1.NgZone, hotkey_service_1.HotkeyService])
], OverlayService);
exports.OverlayService = OverlayService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const http_1 = require('@angular/http');
const Rx_1 = require('rxjs/Rx');
require('rxjs/Rx');
const mirror_change_stream_1 = require('../services/mirror-change.stream');
const connection_service_1 = require('../services/connection.service');
const tab_service_1 = require('../services/tab.service');
const query_result_1 = require('../models/query-result');
const result_page_1 = require('../models/result-page');
const result_store_1 = require('../models/result-store');
const config_1 = require('../config');
let QueryService = class QueryService {
    constructor(tabs, conns, mirror, http) {
        this.port = config_1.default.queryEnginePort;
        this.resultOps = new Rx_1.ReplaySubject();
        this.stream = this.resultOps
            .scan((store, op) => {
            return op(store);
        }, new result_store_1.ResultStore());
        let mirrorWithTab = mirror
            .executing
            .withLatestFrom(tabs.activeBase.filter(x => x !== null && x !== undefined), (queryText, activeTabs) => {
            return {
                tabId: activeTabs[0].id,
                connectionId: activeTabs[0].connectionId,
                text: queryText
            };
        });
        mirrorWithTab
            .subscribe(executing => {
            this.resultOps.next((store) => {
                store.addLoading(executing.tabId);
                return store;
            });
        });
        mirrorWithTab
            .withLatestFrom(conns.all.filter(x => x !== null && x !== undefined), (req, currentConns) => {
            return {
                connectionString: currentConns.find(x => x.id === req.connectionId).connectionString,
                tabId: req.tabId,
                text: req.text
            };
        })
            .flatMap(req => {
            return new Rx_1.Observable((obs) => {
                const mapper = this.extractQueryResult.bind(this);
                http.post(this.action('executequery'), JSON.stringify(req))
                    .map(mapper)
                    .subscribe(data => {
                    data.tabId = req.tabId;
                    data.connectionString = req.connectionString;
                    data.query = req.text;
                    obs.next(data);
                    obs.complete();
                });
            });
        })
            .subscribe(res => {
            this.resultOps.next((store) => {
                store.add(res.tabId, res);
                return store;
            });
        });
        ;
    }
    get activeResult() {
        return this
            .stream;
    }
    setActivePage(queryId, pageId) {
        this.resultOps.next((store) => {
            return store.setActive(queryId, pageId);
        });
    }
    extractQueryResult(res) {
        if (res.status < 200 || res.status >= 300) {
            throw new Error('Bad response status: ' + res.status);
        }
        let result = new query_result_1.QueryResult();
        let body = res.json();
        result.finished = new Date();
        result.id = body.Id;
        Object.keys(body.Results).forEach((key, idx) => {
            const set = body.Results[key];
            const dataRows = set.Item2;
            const dataCols = set.Item1;
            const page = new result_page_1.ResultPage();
            page.columns = dataCols.map(col => col[0]);
            page.columnTypes = dataCols.map(col => col[1]);
            page.rows = dataRows.map((row) => {
                return dataCols.map(col => {
                    return row[col[0]];
                });
            });
            page.title = key;
            page.active = idx === 0; // mark the first as active
            page.id = `${idx}-${result.id}`;
            result.pages.push(page);
        });
        return result;
    }
    action(name) {
        return `http://localhost:${this.port}/${name}`;
    }
};
QueryService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [tab_service_1.TabService, connection_service_1.ConnectionService, mirror_change_stream_1.MirrorChangeStream, http_1.Http])
], QueryService);
exports.QueryService = QueryService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
let StorageService = class StorageService {
    Load(key, defaultValue) {
        try {
            const val = localStorage.getItem(key);
            if (!val) {
                return defaultValue;
            }
            return JSON.parse(val);
        }
        catch (exn) {
            return defaultValue;
        }
    }
    Save(key, value) {
        localStorage.setItem(key, JSON.stringify(value));
    }
};
StorageService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [])
], StorageService);
exports.StorageService = StorageService;

"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
const core_1 = require('@angular/core');
const Rx_1 = require('rxjs/Rx');
const connection_service_1 = require('./connection.service');
let TabService = class TabService {
    constructor(conns) {
        this.conns = conns;
        this.nextId = 0;
        this.ops = new Rx_1.ReplaySubject();
        this.stream = this.ops
            .scan((tabs, op) => {
            let res = op(tabs);
            return res;
        }, []);
        conns.all.subscribe(this.handleConnections.bind(this));
    }
    get tabs() {
        return this.stream;
    }
    get hasTabs() {
        return this
            .tabs
            .map(x => x.length > 0);
    }
    // notify only when active tab id changes
    // first slot is new active,
    // second slot is previous active
    get activeTab() {
        return this.activeBase
            .distinctUntilChanged((x, y) => x[0].id === y[0].id);
    }
    // is notified when
    // - connection id for current tab is changed
    // - active tab is changed
    get active() {
        let detect = this.activeBase
            .map(x => `${x[0].id}:${x[0].connectionId}`)
            .distinctUntilChanged((x, y) => x === y);
        // whenever we detect, we emit the latest from the active tabs list
        return detect.withLatestFrom(this.activeTab, (x, tab) => tab[0]);
    }
    // is notified when a tab enters a new db context for the first time
    get newContext() {
        return this.active
            .scan((ctx, tab) => {
            if (!ctx.conns.find(x => x === tab.id + ':' + tab.connectionId)) {
                ctx.conns = [tab.id + ':' + tab.connectionId, ...ctx.conns];
                ctx.updated = true;
            }
            else {
                ctx.updated = false;
            }
            ctx.tab = tab;
            return ctx;
        }, { tab: null, conns: [], updated: false })
            .filter(ctx => ctx.updated)
            .map(x => x.tab);
    }
    goto(tabId) {
        this.ops.next((tabs) => {
            return this.setActiveTab(tabs, tabId);
        });
    }
    newTab() {
        this.ops.next((tabs) => {
            const conn = tabs.find(t => t.active).connectionId;
            const tab = this.getNewTab(tabs, conn);
            return [
                ...this.setActiveTab(tabs, null),
                tab
            ];
        });
    }
    setConnection(tabId, conn) {
        this.ops.next((tabs) => {
            return tabs.map(x => {
                if (x.id === tabId) {
                    x.connectionId = conn.id;
                }
                return x;
            });
        });
    }
    // handles updates to the tabs as the connections change.
    handleConnections(conns) {
        this.ops.next((tabs) => {
            let oldActive = tabs.find(t => t.active);
            let filtered = tabs.filter(tab => {
                return conns.find(c => c.id === tab.connectionId) !== undefined;
            });
            if (filtered.length > 0 && !filtered.find(x => x.active) && tabs.length !== filtered.length) {
                // set a new active
                filtered[0].active = true;
            }
            else if (filtered.length === 0 && conns.length > 0) {
                // id will take account for old tabs
                filtered.push(this.getNewTab(tabs, conns[0].id));
            }
            let newActive = filtered.find(t => t.active);
            if (newActive && oldActive) {
                filtered = filtered.map(t => {
                    t.previousActive = t.id === oldActive.id;
                    return t;
                });
            }
            return filtered;
        });
    }
    getNewTab(tabs, connectionId, active = true) {
        const id = tabs.reduce((max, val) => val.id >= max ? val.id + 1 : max, 1);
        return {
            id: id,
            active: active,
            connectionId: connectionId,
            title: `Query ${id}`,
        };
    }
    get activeBase() {
        return this.stream
            .filter(ts => ts.length > 0)
            .map(ts => {
            let active = ts.find(t => t.active);
            let prev = ts.find(t => t.previousActive);
            Assert(active, 'no active found');
            return [active, prev];
        });
    }
    setActiveTab(tabs, newId) {
        if (newId === null) {
            // new tab, so just set all inactive, and flag previous
            return tabs.map(t => {
                t.previousActive = t.active;
                t.active = false;
                return t;
            });
        }
        let old = tabs.find(t => t.active);
        if (old && old.id !== newId) {
            return tabs.map(t => {
                t.active = newId === t.id;
                t.previousActive = old.id === t.id;
                return t;
            });
        }
        return tabs;
    }
};
TabService = __decorate([
    core_1.Injectable(), 
    __metadata('design:paramtypes', [connection_service_1.ConnectionService])
], TabService);
exports.TabService = TabService;
