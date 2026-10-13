// Annual fixtures exercise historical years with a deterministic current year.
export async function installReportingClock(page, year = 2028) {
  await page.addInitScript(({ year }) => {
    const NativeDate = Date;
    const now = NativeDate.parse(`${year}-10-13T05:00:00Z`);
    window.Date = class extends NativeDate {
      constructor(...args) { super(...(args.length ? args : [now])); }
      static now() { return now; }
    };
  }, { year });
}
