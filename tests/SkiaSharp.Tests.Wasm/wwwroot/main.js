import { dotnet } from './_framework/dotnet.js'

const { setModuleImports } = await dotnet
  .withDiagnosticTracing(false)
  .withApplicationArgumentsFromQuery()
  .create();

setModuleImports('main.js', {
  xunit: {
    sdk: {
      runner: {
        clearLog: () => {
          var doc = globalThis.document;
          if (!doc)
            return;

          doc.body.innerHTML = ''
        },
        log: (message, type, id) => {
          var doc = globalThis.document;
          if (!doc)
            return;

          if (!message)
            message = '&nbsp;';
          message.replace('\r\n', '<br/>');
          message.replace('\n', '<br/>');

          var attributes = '';
          if (id)
            attributes += 'id="' + id + '" ';
          if (type)
            attributes += 'class="test-' + type + '" ';

          doc.body.innerHTML += '<p ' + attributes + '>' + message + '</p>';
        },
        logResults: (results) => {
          var doc = globalThis.document;
          if (!doc)
            return;

          if (!results)
            results = 'No test results.';
          results.replace('\r\n', '<br/>');
          results.replace('\n', '<br/>');

          doc.body.innerHTML += '<p id="results" class="test-results">' + results + '</p>';
        }
      }
    }
  }
});

await dotnet.run();
