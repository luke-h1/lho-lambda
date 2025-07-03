import { formatDistanceToNow } from 'date-fns';

const streakHandler = (): string => {
  const targetDates = [
    new Date('2025-06-20'),
    new Date('2025-07-02'),
    new Date('2025-07-03'),
  ];

  const distances = targetDates.map(date => ({
    date: date.toISOString(),
    distance: formatDistanceToNow(date, { addSuffix: true }),
  }));

  return JSON.stringify(
    {
      distances,
    },
    null,
    2,
  );
};

export default streakHandler;
