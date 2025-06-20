import { formatDistanceToNow } from 'date-fns';

const streakHandler = (): string => {
  const targetDate = new Date('2025-06-20');

  const distance = formatDistanceToNow(targetDate, { addSuffix: true });

  return JSON.stringify(
    {
      distance,
    },
    null,
    2,
  );
};

export default streakHandler;
