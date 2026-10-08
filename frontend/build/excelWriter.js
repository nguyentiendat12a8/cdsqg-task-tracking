import { readFile } from 'node:fs/promises';
import path from 'node:path';

// The standalone writer and full reader are separate package entry points.
// XLSX exports use Unicode XML; old XLS codepages belong to the import path.
function withoutLegacyCodepages(code) {
  const dependency = 'require("./cpexcel.js")';
  if (!code.includes(dependency)) throw new Error('Excel writer entry changed; review the codepage split.');
  return code.replace(dependency, 'undefined');
}

export function excelWriterPlugin() {
  return {
    name: 'excel-writer-without-legacy-codepages',
    enforce: 'pre',
    transform(code, id) {
      if (id.replaceAll('\\', '/').endsWith('/xlsx-js-style/dist/xlsx.bundle.js')) {
        return { code: withoutLegacyCodepages(code), map: null };
      }
    }
  };
}

export function excelWriterDependencyPlugin() {
  return {
    name: 'excel-writer-without-legacy-codepages',
    setup(build) {
      build.onLoad({ filter: /xlsx-js-style[/\\]dist[/\\]xlsx\.bundle\.js$/ }, async ({ path: file }) => ({
        contents: withoutLegacyCodepages(await readFile(file, 'utf8')),
        loader: 'js',
        resolveDir: path.dirname(file)
      }));
    }
  };
}
