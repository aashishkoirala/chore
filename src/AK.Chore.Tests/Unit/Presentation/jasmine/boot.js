/**
 Starting with version 2.0, this file "boots" Jasmine, performing all of the necessary initialization before executing the loaded environment and all of a project's specs. This file should be loaded after `jasmine.js` and `jasmine_html.js`, but before any project source files or spec files are loaded. Thus this file can also be used to customize Jasmine for a project.

 If a project is using Jasmine via the standalone distribution, this file can be customized directly. If a project is using Jasmine via the [Ruby gem][jasmine-gem], this file can be copied into the support directory via `jasmine copy_boot_js`. Other environments (e.g., Python) will have different mechanisms.

 The location of `boot.js` can be specified and/or overridden in `jasmine.yml`.

 [jasmine-gem]: http://github.com/pivotal/jasmine-gem
 */

(function() {

  /**
   * ## Require &amp; Instantiate
   *
   * Require Jasmine's core files. Specifically, this requires and attaches all of Jasmine's code to the `jasmine` reference.
   */
  window.jasmine = jasmineRequire.core(jasmineRequire);

  /**
   * Since this is being run in a browser and the results should populate to an HTML page, require the HTML-specific Jasmine code, injecting the same reference.
   */
  jasmineRequire.html(jasmine);

  /**
   * Create the Jasmine environment. This is used to run all specs in a project.
   */
  var env = jasmine.getEnv();

  /**
   * ## The Global Interface
   *
   * Build up the functions that will be exposed as the Jasmine public interface. A project can customize, rename or alias any of these functions as desired, provided the implementation remains unchanged.
   */
  var jasmineInterface = jasmineRequire.interface(jasmine, env);

  /**
   * Add all of the Jasmine global/public interface to the proper global, so a project can use the public interface directly. For example, calling `describe` in specs instead of `jasmine.getEnv().describe`.
   */
  if (typeof window == "undefined" && typeof exports == "object") {
    extend(exports, jasmineInterface);
  } else {
    extend(window, jasmineInterface);
  }

  /**
   * ## Runner Parameters
   *
   * More browser specific code - wrap the query string in an object and to allow for getting/setting parameters from the runner user interface.
   */

  var queryString = new jasmine.QueryString({
    getWindowLocation: function() { return window.location; }
  });

  var catchingExceptions = queryString.getParam("catch");
  env.catchExceptions(typeof catchingExceptions === "undefined" ? true : catchingExceptions);

  /**
   * ## Reporters
   * The `HtmlReporter` builds all of the HTML UI for the runner page. This reporter paints the dots, stars, and x's for specs, as well as all spec names and all failures (if any).
   */
  var htmlReporter = new jasmine.HtmlReporter({
    env: env,
    onRaiseExceptionsClick: function() { queryString.setParam("catch", !env.catchingExceptions()); },
    getContainer: function() { return document.body; },
    createElement: function() { return document.createElement.apply(document, arguments); },
    createTextNode: function() { return document.createTextNode.apply(document, arguments); },
    timer: new jasmine.Timer()
  });

  /**
   * The `jsApiReporter` also receives spec results, and is used by any environment that needs to extract the results  from JavaScript.
   */
  env.addReporter(jasmineInterface.jsApiReporter);
  env.addReporter(htmlReporter);

  /**
   * Filter which specs will be run by matching the start of the full name against the `spec` query param.
   */
  var specFilter = new jasmine.HtmlSpecFilter({
    filterString: function() { return queryString.getParam("spec"); }
  });

  env.specFilter = function(spec) {
    return specFilter.matches(spec.getFullName());
  };

  /**
   * Setting up timing functions to be able to be overridden. Certain browsers (Safari, IE 8, phantomjs) require this hack.
   */
  window.setTimeout = window.setTimeout;
  window.setInterval = window.setInterval;
  window.clearTimeout = window.clearTimeout;
  window.clearInterval = window.clearInterval;


    ////// START OF MODIFICATIONS for CHORE //////
    
    /* The following modifications made to work with RequireJS. Logic taken from the post at: http://stackoverflow.com/a/21635969/2662221 */
        
  /**
   * ## Execution
   *
   * Replace the browser window's `onload`, ensure it's called, and then run all of the loaded specs. This includes initializing the `HtmlReporter` instance and then executing the loaded Jasmine environment. All of this will happen after all of the specs are loaded.
   */
  var currentWindowOnload = window.onload;

    // Stack of AMD spec definitions
  var specDefinitions = [];

    // Store a ref to the current require function
  window.oldRequire = require;

    // Shim in our Jasmine spec require helper, which will queue up all of the definitions to be loaded in later.
  require = function (deps, specCallback) {
      //push any module defined using require([deps], callback) onto the specDefinitions stack.
      specDefinitions.push({ 'deps': deps, 'specCallback': specCallback });
  };

    //
  window.onload = function () {

      // Restore original require functionality
      window.require = oldRequire;
      // Keep a ref to Jasmine context for when we execute later
      var context = this,
          requireCalls = 0, // counter of (successful) require callbacks
          specCount = specDefinitions.length; // # of AMD specs we're expecting to load

      // func to execute the AMD callbacks for our test specs once requireJS has finished loading our deps
      function execSpecDefinitions() {
          //exec the callback of our AMD defined test spec, passing in the returned modules.
          this.specCallback.apply(context, arguments);
          requireCalls++; // inc our counter for successful AMD callbacks.
          if (requireCalls === specCount) {
              //do the normal Jamsine HTML reporter initialization
              htmlReporter.initialize.call(context);
              //execute our Jasmine Env, now that all of our dependencies are loaded and our specs are defined.
              env.execute.call(context);
          }
      }

      // iterate through all of our AMD specs and call require with our spec execution callback
      for (var i = specDefinitions.length - 1; i >= 0; i--) {
          require(specDefinitions[i].deps, execSpecDefinitions.bind(specDefinitions[i]));
      }

      //keep original onload in case we set one in the HTML
      if (currentWindowOnload) {
          currentWindowOnload();
      }

      // The following only works if you have Commons library also checked out.
      
      require.config({
          paths: {
              jquery: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\jquery-2.1.1.min',
              bootstrap: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\bootstrap.min',
              angular: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\angular.min',
              ngRoute: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\angular-route.min',
              ngResource: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\angular-resource.min',
              uiBootstrap: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\ui-bootstrap-tpls-0.11.0.min',
              akUiAngular: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\ak-ui-angular',
              signalR: '..\\..\\..\\..\\..\\Commons\\src\\AK.Commons.Web\\LibraryResources\\JavaScript\\jquery.signalR-2.1.2.min',
              ngMock: 'angular-mocks',
              signalRHubs: 'about:blank',
              app: '..\\..\\..\\AK.Chore.Presentation\\Client\\app',
              'api.service': '..\\..\\..\\AK.Chore.Presentation\\Client\\api.service',
              'folder.service': '..\\..\\..\\AK.Chore.Presentation\\Client\\folders\\folder.service',
              'filter.service': '..\\..\\..\\AK.Chore.Presentation\\Client\\filters\\filter.service',
              'user.service': '..\\..\\..\\AK.Chore.Presentation\\Client\\user\\user.service',
              'calendar.service': '..\\..\\..\\AK.Chore.Presentation\\Client\\calendar\\calendar.service',
              'task.service': '..\\..\\..\\AK.Chore.Presentation\\Client\\tasks\\task.service'
          },
          shim: {
              jquery: { exports: 'jquery' },
              bootstrap: { deps: ['jquery'], exports: 'bootstrap' },
              angular: { deps: ['jquery'], exports: 'angular' },
              ngRoute: { deps: ['angular'], exports: 'ngRoute' },
              ngResource: { deps: ['angular'], exports: 'ngResource' },
              uiBootstrap: { deps: ['angular', 'bootstrap'], exports: 'uiBootstrap' },
              akUiAngular: { deps: ['angular'], exports: 'akUiAngular' },
              signalR: { deps: ['jquery'], exports: 'signalR' },
              signalRHubs: { deps: ['signalR'], exports: 'signalRHubs' },
              ngMock: { deps: ['angular'] }
          }
      });

      htmlReporter.initialize();

      require(['folder.service.tests', 'filter.service.tests', 'user.service.tests', 'calendar.service.tests', 'task.service.tests'], function() {
          env.execute();
      });
  };
    
  /**
   * Helper function for readability above.
   */
  function extend(destination, source) {
    for (var property in source) destination[property] = source[property];
    return destination;
  }

}());