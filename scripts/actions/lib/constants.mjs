import { createRequire } from 'node:module';
import * as path from 'path';
import * as url from 'url';

// eslint-disable-next-line no-underscore-dangle
const __dirname = path.dirname(url.fileURLToPath(import.meta.url));

export const workspaceRoot = path.resolve(__dirname, '../../../');
export const workspaces = ['apps/*'];
export const require = createRequire(import.meta.url);
