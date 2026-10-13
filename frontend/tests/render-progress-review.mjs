import fs from 'node:fs/promises';
import {FileBlob,SpreadsheetFile} from 'file:///C:/Users/84878/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load('D:/CDSQG/Source thử nghiệm theo dõi chiến lược/frontend/tests/progress-template-review.xlsx'));
const preview=await workbook.render({sheetName:'Báo Cáo Tiến Độ',range:'A1:H6',scale:1,format:'png'});
await fs.writeFile('D:/CDSQG/Source thử nghiệm theo dõi chiến lược/frontend/tests/progress-template-review.png',new Uint8Array(await preview.arrayBuffer()));
