// eslint-disable-next-line @typescript-eslint/no-unused-vars
const lambdaActions = {
  nowPlaying: 'reqNowPlaying',
  health: 'reqHealth',
  version: 'reqVersion',
  unknown: 'unknown',
} as const;

export type LambdaActions = keyof typeof lambdaActions;

interface Errors extends Error {
  statusCode?: number;
  code?: number;
  body?: string;
  action?: LambdaActions;
}

export default class LambdaError extends Error implements Errors {
  statusCode?: number;

  code?: number;

  body?: string;

  action?: LambdaActions;

  constructor({
    message,
    name,
    action,
    body,
    code,
    stack,
    statusCode,
  }: Errors) {
    super(message);
    this.name = name || 'LambdaError';
    this.action = action || 'unknown';
    this.body = body;
    this.code = code;
    this.stack = stack;
    this.statusCode = statusCode;

    // Prevent: TypeError: Object.setPrototypeOf called on null or undefined
    Object.setPrototypeOf(this, LambdaError.prototype);
  }
}
