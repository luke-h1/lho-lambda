#!/usr/bin/env node

import { execa } from 'execa';
import glob from 'glob';

import {
  getPackageManifest,
  updatePackageManifest,
  listPackages,
} from '../actions/lib/packages.mjs';

const versionRe = /^\d+\.\d+\.\d+/i;
const execaOpts = { stdio: 'inherit' };

await execa('changeset', ['version'], execaOpts);
await execa('pnpm', ['install', '--lockfile-only'], execaOpts);

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const packages = (await listPackages()).reduce((map, dir) => {
  const manifest = getPackageManifest(dir);
  const versionMatch = manifest.version.match(versionRe);
  if (versionMatch) {
    const { name } = manifest;
    const version = `^${versionMatch[0]}`;
    // eslint-disable-next-line no-param-reassign
    map[name] = version;
  }
  return map;
}, {});
