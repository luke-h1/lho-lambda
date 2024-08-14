/** @type {import('jest').Config} */
const config = {
  preset: 'ts-jest/presets/default-esm',
  testEnvironment: 'node',
  collectCoverageFrom: ['src/**/*.{js,jsx,ts,tsx}', '!src/**/*.d.ts'],
  testMatch: [
    '<rootDir>/src/**/*.(test).{js,jsx,ts,tsx}',
    '<rootDir>/test/**/*.(test).{js,jsx,ts,tsx}',
  ],
  verbose: true,
  resetMocks: true,
  testPathIgnorePatterns: ['/node_modules/', '/dist/', 'e2e'],
  transform: {
    '^.+\\.(ts|js)$': 'ts-jest',
  },
  moduleNameMapper: {
    '^@lambda/(.*)$': '<rootDir>/src/$1',
    '^@lambda-test/(.*)$': '<rootDir>/test/$1',
  },
  setupFilesAfterEnv: ['<rootDir>/src/test/setupTests.js'],
  coverageThreshold: {
    global: {
      statements: 93.8,
      branches: 85,
      lines: 95,
      functions: 96,
    },
  },
};
module.exports = config;
