import { describe, expect, test } from 'bun:test';
import { mkdtempSync, readFileSync, readdirSync, writeFileSync, mkdirSync, existsSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const SCRIPT = join(import.meta.dir, 'release-notes.ts');

function repo(changesets: Record<string, string>): string {
  const dir = mkdtempSync(join(tmpdir(), 'release-notes-'));
  mkdirSync(join(dir, '.changeset'));
  writeFileSync(
    join(dir, '.changeset', 'config.json'),
    JSON.stringify({ baseBranch: 'main' }),
  );
  writeFileSync(join(dir, 'CHANGELOG.md'), '# Changelog\n');
  for (const [name, body] of Object.entries(changesets)) {
    writeFileSync(join(dir, '.changeset', `${name}.md`), body);
  }
  return dir;
}

function run(dir: string, ...args: string[]): string {
  const result = Bun.spawnSync(['bun', SCRIPT, ...args], { cwd: dir });
  if (result.exitCode !== 0) {
    throw new Error(result.stderr.toString().trim());
  }
  return result.stdout.toString();
}

const note = (bump: string, summary: string) =>
  `---\n'lho-lambda': ${bump}\n---\n\n${summary}\n`;

describe('pending', () => {
  test('prints the no-notes line when nothing is waiting', () => {
    const dir = repo({});
    expect(run(dir, 'pending').trim()).toBe(
      'No release notes are waiting since the last release.',
    );
  });

  test('groups the notes by bump heading', () => {
    const dir = repo({
      a: note('minor', 'Adds a recent-tracks endpoint.'),
      b: note('patch', 'Caches the now-playing lookup.'),
      c: note('major', 'Drops the legacy payload.'),
    });
    expect(run(dir, 'pending')).toBe(
      [
        '### Breaking changes',
        '',
        '- Drops the legacy payload.',
        '',
        '### New',
        '',
        '- Adds a recent-tracks endpoint.',
        '',
        '### Fixed',
        '',
        '- Caches the now-playing lookup.',
        '',
      ].join('\n'),
    );
  });
});

describe('release', () => {
  test('writes the section under the version and deletes the files', () => {
    const dir = repo({
      a: note('minor', 'Adds a recent-tracks endpoint.'),
      empty: '---\n---\n',
    });
    const printed = run(
      dir,
      'release',
      '--version',
      'live.abc1234',
      '--date',
      '2026-11-17',
    );
    const changelog = readFileSync(join(dir, 'CHANGELOG.md'), 'utf8');
    expect(changelog).toBe(
      '# Changelog\n\n## live.abc1234 - 2026-11-17\n\n### New\n\n- Adds a recent-tracks endpoint.\n',
    );
    expect(printed.trim()).toBe(
      '## live.abc1234 - 2026-11-17\n\n### New\n\n- Adds a recent-tracks endpoint.',
    );
    expect(readdirSync(join(dir, '.changeset')).sort()).toEqual([
      'config.json',
    ]);
  });

  test('records a release with only empty changesets', () => {
    const dir = repo({ empty: '---\n---\n' });
    run(dir, 'release', '--version', 'live.abc1234', '--date', '2026-11-17');
    expect(readFileSync(join(dir, 'CHANGELOG.md'), 'utf8')).toBe(
      '# Changelog\n\n## live.abc1234 - 2026-11-17\n\nThis release has no user-facing changes.\n',
    );
    expect(existsSync(join(dir, '.changeset', 'empty.md'))).toBe(false);
  });

  test('puts the newest release first', () => {
    const dir = repo({ a: note('patch', 'First.') });
    run(dir, 'release', '--version', 'live.aaaaaaa', '--date', '2026-11-16');
    writeFileSync(join(dir, '.changeset', 'b.md'), note('patch', 'Second.'));
    run(dir, 'release', '--version', 'live.bbbbbbb', '--date', '2026-11-17');
    const changelog = readFileSync(join(dir, 'CHANGELOG.md'), 'utf8');
    expect(changelog.indexOf('live.bbbbbbb')).toBeLessThan(
      changelog.indexOf('live.aaaaaaa'),
    );
  });
});

describe('show', () => {
  test('prints the body of a recorded section', () => {
    const dir = repo({ a: note('patch', 'Caches the now-playing lookup.') });
    run(dir, 'release', '--version', 'live.abc1234', '--date', '2026-11-17');
    expect(run(dir, 'show', '--version', 'live.abc1234').trim()).toBe(
      '### Fixed\n\n- Caches the now-playing lookup.',
    );
  });

  test('fails when the version has no section', () => {
    const dir = repo({});
    expect(() => run(dir, 'show', '--version', 'live.abc1234')).toThrow(
      'CHANGELOG.md has no section for live.abc1234',
    );
  });
});

describe('flags', () => {
  test('fails when a flag has no value', () => {
    const dir = repo({});
    expect(() => run(dir, 'release', '--version')).toThrow(
      '--version needs a value',
    );
  });

  test('rejects a level that is not a positive integer', () => {
    const dir = repo({});
    expect(() => run(dir, 'pending', '--level', '0')).toThrow(
      '--level must be a positive integer',
    );
  });
});
