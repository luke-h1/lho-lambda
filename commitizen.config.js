module.exports = {
  types: [
    { value: 'feat', name: 'feat:\tAdding a new feature' },
    { value: 'fix', name: 'fix:\tFixing a bug' },
    {
      value: 'style',
      name: 'style: Add or update styles',
    },
    {
      value: 'refactor',
      name: 'refactor:\tCode change that neither fixes a bug nor adds a feature',
    },
    {
      value: 'perf',
      name: 'perf:\tCode change that improves performance',
    },
    {
      value: 'test',
      name: 'test:\tAdding tests cases / adds missing tests',
    },
    {
      value: 'chore',
      name: 'chore:\tChanges to the build process or auxiliary tools',
    },
    { value: 'revert', name: 'revert:\tRevert to a commit' },
  ],
  scopes: [
    { name: 'api' },
    { name: 'lambda' },
    { name: 'authorizer' },
    { name: 'observability' },
    { name: 'infrastructure' },
    { name: 'documentation' },
    { name: 'ci' },
  ],
  allowCustomScopes: true,
  allowBreakingChanges: ['feat', 'fix', 'perf', 'refactor'],
  subjectLimit: 100,
  skipQuestions: ['body'],
};
