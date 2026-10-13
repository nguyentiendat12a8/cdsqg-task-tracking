import fs from 'node:fs';
import path from 'node:path';
const root=path.resolve('frontend/src');
const files=[];function walk(dir){for(const e of fs.readdirSync(dir,{withFileTypes:true})){const p=path.join(dir,e.name);if(e.isDirectory())walk(p);else if(e.name.endsWith('.vue'))files.push(p)}}walk(root);
let changed=0;
for(const file of files){let text=fs.readFileSync(file,'utf8');const already=/\bformatNumber\b/.test(text);let count=0;
text=text.replace(/\{\{\s*([\w.$]+(?:\s*(?:\|\||\?\?)\s*0)?)\s*\}\}/g,(original,expression)=>{
 const field=expression.split('.').at(-1).replace(/\s*(?:\|\||\?\?)\s*0$/,'');
 if(!/^(?:[a-zA-Z]+Count|count|specific|general|activeStatusTotal|total(?:Count|Goals|Tasks|Items|ReportsSubmitted|GoalsSum|TasksSum|GridItems)?|grandTotal(?:Items|Completed|Overdue)|currentActiveTotalCount|(?:success|pending|failure|urged|editingUsed|validApproved|validPending|validTotal|invalid|parsed|approved|rejected|completed|notStarted|inProgress|overdue)Count|completedOnTime|completedOverdue|inProgressOnTime|inProgressOverdue|expiringSoon|notStarted)$/.test(field))return original;
 count++;return '{{ formatNumber('+expression+') }}';
});
if(count){if(!already){const rel=path.relative(path.dirname(file),path.join(root,'shared/formatters')).replaceAll('\\','/');text=text.replace(/<script setup(?:\s[^>]*)?>/,m=>m+'\nimport { formatNumber } from '+JSON.stringify(rel.startsWith('.')?rel:'./'+rel)+';');}fs.writeFileSync(file,text);changed++;}}
console.log('Normalized quantity/count text in '+changed+' components.');
