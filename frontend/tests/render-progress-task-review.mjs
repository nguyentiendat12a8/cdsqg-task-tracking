import fs from 'node:fs/promises';
import {FileBlob,SpreadsheetFile} from 'file:///C:/Users/84878/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const workbook=await SpreadsheetFile.importXlsx(await FileBlob.load('D:/CDSQG/Source thử nghiệm theo dõi chiến lược/frontend/tests/progress-task-template-review.xlsx'));
const preview=await workbook.render({sheetName:'Báo Cáo Tiến Độ',range:'A1:I7',scale:1,format:'png'});
await fs.writeFile('D:/CDSQG/Source thử nghiệm theo dõi chiến lược/frontend/tests/progress-task-template-review.png',new Uint8Array(await preview.arrayBuffer()));

