import { normalizeExecutionStatus, getStatusLabel } from '../shared/statusPresentation.js';

const text = value => String(value ?? '').replace(/\r\n?/g, '\n').trim().replace(/^—$/, '');
const numeric = value => text(value) === '' ? null : Number(String(value).replace('%', '').replace(',', '.'));
const sameNumber = (a, b) => numeric(a) === numeric(b);
const yearKey = year => year === 2025 ? 'actual2025' : year === 2026 ? 'estimate2026' : `plan${year}`;

// Compare against the same report snapshot used to parse the workbook, never against row counts.
export function reconcileProgressReportImport(parsed, goals, tasks) {
  const preview = { goals: [], tasks: [] };
  const rows = new Map();
  const goalRow = goal => {
    if (!rows.has(goal.id)) {
      const row = { ...goal, id: goal.id, code: goal.subCodes?.join(' / ') || goal.code, title: goal.title, documentNumber: goal.documentNumber, cells: { targetPhase1: { value: goal.targetPhase1 }, targetPhase2: { value: goal.targetPhase2 } } };
      for (const year of [2025, 2026, 2027, 2028, 2029, 2030]) row.cells[year] = { value: goal[yearKey(year)], changed: false };
      row.cells.notes = { value: goal.notes, changed: false };
      rows.set(goal.id, row);
    }
    return rows.get(goal.id);
  };
  const changedGoals = parsed.goals.filter(item => {
    const goal = goals.find(g => g.id === item.id || g.subGoalIds?.includes(item.id));
    if (!goal) return false;
    const old = goal[yearKey(item.periodYear)];
    const valueChanged = !sameNumber(old, item.value);
    const notesChanged = text(goal.notes) !== text(item.summaryNotes);
    if (!valueChanged && !notesChanged) return false;
    const row = goalRow(goal);
    row.cells[item.periodYear] = { value: item.value, previous: old, changed: valueChanged };
    row.cells.notes = { value: item.summaryNotes, previous: goal.notes, changed: notesChanged };
    return true;
  });
  const changedPlans = parsed.plans.filter(item => {
    const goal = goals.find(g => g.id === item.taskId || g.subGoalIds?.includes(item.taskId));
    if (!goal || sameNumber(goal[yearKey(item.year)], item.targetQuantity)) return false;
    goalRow(goal).cells[item.year] = { value: item.targetQuantity, previous: goal[yearKey(item.year)], changed: true };
    return true;
  });
  const changedTasks = parsed.tasks.filter(item => {
    const task = tasks.find(t => t.id === item.id);
    if (!task) return false;
    const cells = {
      status: { value: getStatusLabel(item.status), previous: getStatusLabel(task.status), changed: normalizeExecutionStatus(item.status) !== normalizeExecutionStatus(task.status) },
      notes: { value: item.summaryNotes, previous: task.notes, changed: text(item.summaryNotes) !== text(task.notes) }
    };
    const percentChanged = item.value != null && !sameNumber(item.value, task.completionPercentage);
    if (!Object.values(cells).some(c => c.changed) && !percentChanged) return false;
    if (percentChanged) cells.percent = { value: item.value, previous: task.completionPercentage, changed: true };
    preview.tasks.push({ ...task, id: task.id, code: task.code, title: task.title, documentNumber: task.documentNumber, cells: { ...cells, dueDate: { value: task.dueDate }, deliverables: { value: task.deliverables } } });
    return true;
  });
  preview.goals = [...rows.values()];
  return { ...parsed, goals: changedGoals, plans: changedPlans, tasks: changedTasks, preview };
}
