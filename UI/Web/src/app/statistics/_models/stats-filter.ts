
export type StatsFilter = {
  timeFilter: {
    startTime: Date | null,
    endTime: Date | null,
  },
  libraries: number[],
}
