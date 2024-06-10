#!/usr/bin/env node

import { buildPackage } from './lib/commands.mjs';
import { listPackages } from './lib/packages.mjs';

(async () => {
  try {
    const packages = await listPackages();
    const builds = packages.map(buildPackage);
    await Promise.all(builds);
  } catch (e) {
    // eslint-disable-next-line no-console
    console.error(e.message);
    process.exit(1);
  }
})();
