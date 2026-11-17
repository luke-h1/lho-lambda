# Changelog

## live.c480282 - 2026-11-17

This release has no user-facing changes.

## live.150c1cf - 2026-11-17

### Fixed

- Last.fm failures now say what is wrong. When the configured account hides its recent listening, does not exist, or the API key is rejected, the error names the account and the setting or variable to change instead of a bare HTTP 403. `/api/now-playing` still returns the empty model with `status: 500` until that is resolved.

## live.e162ddb - 2026-11-15

### Fixed

- **api:** top tracks + now playing (#221) ([e162ddb](https://github.com/luke-h1/lho-lambda/commit/e162ddbbe48dacaf10464f3e7b372b71e4649997))

## live.4037bc6 - 2026-07-22

### New

- **api:** add recent-tracks endpoint (#219) ([4037bc6](https://github.com/luke-h1/lho-lambda/commit/4037bc6736c1cac7f8d4cfeb906f7d0b77f95be8))

## live.077b233 - 2026-07-05

### Other changes

- **api:** update to dotnet 10 (#217) ([26c47f7](https://github.com/luke-h1/lho-lambda/commit/26c47f77d4dba617c78a11cebfe857c1a2b321c5))
- **api:** remote push gw (#218) ([077b233](https://github.com/luke-h1/lho-lambda/commit/077b233f1f78a0710ab9a747cdb55c0a84ebad3a))

## live.de19d60 - 2026-05-16

### Other changes

- **api:** improve codebase structure ([b54526c](https://github.com/luke-h1/lho-lambda/commit/b54526c59414a5725e6816fc7611fea50304e4e5))
- **api:** improve codebase structure ([7166bda](https://github.com/luke-h1/lho-lambda/commit/7166bda0be9deab5d0cea39b5ccb884fb81d8e4f))
- **api:** improve codebase structure ([d056966](https://github.com/luke-h1/lho-lambda/commit/d05696613016a14b7e53302567bd39e4fd5a84df))

## live.772bbc4 - 2026-05-14

### Other changes

- **api:** add c# formatting to pre-commit ([772bbc4](https://github.com/luke-h1/lho-lambda/commit/772bbc4e82915fa0846bf62250d6ea4e1f30f960))

## live.611ba63 - 2026-05-14

### New

- **lambda:** add observability ([f4630b2](https://github.com/luke-h1/lho-lambda/commit/f4630b2c6ab20556f5a5ce47d7c727c50cf8b8b7))
- **lambda:** add observability ([c76f7fa](https://github.com/luke-h1/lho-lambda/commit/c76f7fa64a0e632b362fe4a3d1656e9aee8ba157))
- **lambda:** add observability ([9226f00](https://github.com/luke-h1/lho-lambda/commit/9226f006afbd2633c04332a0d1f1b0b435e23f7e))

## live.65ec19f - 2026-05-13

### New

- **lambda:** rewrite lambdas ([2ec5bdb](https://github.com/luke-h1/lho-lambda/commit/2ec5bdbc2c801381348cfa18970e8f0f28331a22))
- **lambda:** convert project to swift ([132af2b](https://github.com/luke-h1/lho-lambda/commit/132af2b78f38996017b903fab56c7329b56b5e32))
- **lambda:** convert project to swift ([b44dbf2](https://github.com/luke-h1/lho-lambda/commit/b44dbf2d697a86b43b20f592182fc32cbed199e6))
- **lambda:** convert project to swift ([650dad9](https://github.com/luke-h1/lho-lambda/commit/650dad9e0e62f4ee59be82f4115ef29e4a204347))
- **lambda:** convert project to swift ([f0e874a](https://github.com/luke-h1/lho-lambda/commit/f0e874a26f8662ee040d16b4199e46346483cebe))
- **lambda:** convert to swift ([4bcf5ec](https://github.com/luke-h1/lho-lambda/commit/4bcf5ec1ff78c8cb69dc0fdd8848e9f6a9bf7ef4))

### Fixed

- **api:** fix secrets not being loaded ([404bfb2](https://github.com/luke-h1/lho-lambda/commit/404bfb21d0daf62c89ebc14feb5513dffa104afe))
- **api:** fix secrets not being loaded ([98adecd](https://github.com/luke-h1/lho-lambda/commit/98adecd3d425d5cafbcfc95fd926fb57a68c6335))
- **api:** fix secrets not being loaded ([577f4f9](https://github.com/luke-h1/lho-lambda/commit/577f4f9ac3f0a8639327361a74c8c58cd242b14c))
- **api:** fix secrets not being loaded ([399195d](https://github.com/luke-h1/lho-lambda/commit/399195d72bb0fa45ed29e2b8ecae1d0d208293e7))
- **api:** fix secrets not being loaded ([3c717e9](https://github.com/luke-h1/lho-lambda/commit/3c717e9515d5c47e68511d79834b11adbf611b3f))
- **api:** fix secrets not being loaded ([41ba8e1](https://github.com/luke-h1/lho-lambda/commit/41ba8e1137113d0d8ba3350e3e546254b1e1e498))
- **api:** fix secrets not being loaded ([29bebd0](https://github.com/luke-h1/lho-lambda/commit/29bebd03ba5cf13ba500ee70a57dccde6ff658e7))
- **api:** fix secrets not being loaded ([17021df](https://github.com/luke-h1/lho-lambda/commit/17021dfab6c469b6ded9dc98aaa7fe3914e3bcf3))
- **api:** fix spotify 500 ([c61a65e](https://github.com/luke-h1/lho-lambda/commit/c61a65e7cfac9691d7cee8dc4ee9ce98d2252213))
- **api:** fix spotify 500 ([67be0b8](https://github.com/luke-h1/lho-lambda/commit/67be0b8ffea7337313d0943ffd2d490e8a1196ec))

### Other changes

- **infrastructure:** add test to destroy opts ([9e8632d](https://github.com/luke-h1/lho-lambda/commit/9e8632d81e42765d4252f4ddfd5bb8d67a26c037))
- **infrastructure:** add test to destroy opts ([03844f2](https://github.com/luke-h1/lho-lambda/commit/03844f2e8e6df3d44320bf6e8d07f546a5d4e8b1))
- **api:** debug issues on prod lambda ([e6b8060](https://github.com/luke-h1/lho-lambda/commit/e6b80606030661e6f69d7f4b687becae04fc63b6))
- **api:** debug issues on prod lambda ([cb91840](https://github.com/luke-h1/lho-lambda/commit/cb91840897833356c68bea9c7187a3cb3de08e50))
- **lambda:** update lambda deps ([06508be](https://github.com/luke-h1/lho-lambda/commit/06508be14c9183c81a79e449a059bf7aa05d88f5))
- **lambda:** update deps ([85e7bee](https://github.com/luke-h1/lho-lambda/commit/85e7bee92f889e16c513cd924e1f872474455df4))
- **lambda:** update deps ([23a3157](https://github.com/luke-h1/lho-lambda/commit/23a31575fe1e543e73ed3fd6a90187ea912d7394))
- **lambda:** update deps ([d369a37](https://github.com/luke-h1/lho-lambda/commit/d369a371eace5c4bc4074db260a95295a2bfd642))
- **api:** lastfm ([92a5055](https://github.com/luke-h1/lho-lambda/commit/92a50556ef1bdacab2ac19055b18a0e9e6d56802))
- **api:** lastfm ([6f78696](https://github.com/luke-h1/lho-lambda/commit/6f786965fad694e2dd3dca9a94543d13c9e78dc7))
- **api:** lastfm ([9795622](https://github.com/luke-h1/lho-lambda/commit/9795622ed8b144bf904c9d664593a87d6a5a6632))
- **api:** lastfm ([18fe61c](https://github.com/luke-h1/lho-lambda/commit/18fe61c8b49e00f869f05e69f7272274455e4f03))

## 1.4.2 - 2025-07-03

### Other changes

- **api:** change dates ([ec5bc62](https://github.com/luke-h1/lho-lambda/commit/ec5bc6251969086f0013cdcc45ea4fcee9d1a978))

## 1.4.1 - 2025-07-03

### Other changes

- **lambda:** change streak format ([d35bac8](https://github.com/luke-h1/lho-lambda/commit/d35bac8ccdbfb78a41fe458717061d4051499dd7))

## 1.4.0 - 2025-06-20

### New

- **api:** add streak handler ([8c1fbc1](https://github.com/luke-h1/lho-lambda/commit/8c1fbc11208ef6d68ad8b1a0600bea9650be8907))

## 1.3.0 - 2025-06-20

### New

- **api:** add streak handler ([8749386](https://github.com/luke-h1/lho-lambda/commit/8749386ac0025ec127fbc0f66e11ed2d963f38e7))

## 1.2.0 - 2025-05-10

### New

- **infrastructure:** convert to bun ([4825ab6](https://github.com/luke-h1/lho-lambda/commit/4825ab64231ca2b56987db87c11ba6016dc14cfc))

## 1.1.2 - 2025-05-06

### Other changes

- **api:** consumer header validation ([5c1f96d](https://github.com/luke-h1/lho-lambda/commit/5c1f96d32aed046a918b62faec80ce9de629afbd))

## 1.1.1 - 2025-05-04

### Other changes

- **infrastructure:** align auth lambda settings ([f96b8dc](https://github.com/luke-h1/lho-lambda/commit/f96b8dc99bd26cdd0d00639a039aec9d860d632d))

## 1.1.0 - 2025-05-03

### New

- **gateway:** protect api with auth ([2fed80e](https://github.com/luke-h1/lho-lambda/commit/2fed80e8d4b679b1249b8494e15a6b2487cbebed))

## 0.3.57 - 2025-04-16

### Other changes

- **lambda:** error handling tweaks ([844b9c4](https://github.com/luke-h1/lho-lambda/commit/844b9c49908e8c0c43c9ebe5957617fb28ca4cd4))

## 0.3.56 - 2025-04-16

### Performance

- **lambda:** increase memory size to 256mb ([d857fc8](https://github.com/luke-h1/lho-lambda/commit/d857fc86849e77377653ed72844345a6068bac45))
- **lambda:** increase memory size to 256mb ([9cf32e9](https://github.com/luke-h1/lho-lambda/commit/9cf32e9d7c94d655d3b3418f9d2869edcbda7574))
- **lambda:** increase memory size to 256mb ([4416322](https://github.com/luke-h1/lho-lambda/commit/4416322a25615dfc9c487fc8626ea07fda13d9d4))
- **lambda:** increase memory size to 256mb ([395589e](https://github.com/luke-h1/lho-lambda/commit/395589e32c7a77db5b56b0f4b4bddd4315fcd593))

## 0.3.55 - 2025-04-01

### Fixed

- **test:** fix route unit test ([2cd9304](https://github.com/luke-h1/lho-lambda/commit/2cd930401e4c11119c753533cb66d8ab951d3116))

### Other changes

- **gateway:** reduce cache time ([9213636](https://github.com/luke-h1/lho-lambda/commit/92136365f3acc0a1144dd240e4895a798cbf260d))

## 0.3.54 - 2025-02-12

### Other changes

- **lambda:** tighten path type ([345a1d6](https://github.com/luke-h1/lho-lambda/commit/345a1d6e817264cf5cc59cb34dc5a316f7b4cc5b))

## 0.3.53 - 2025-02-12

### Fixed

- **ci:** fix build ([ab613ea](https://github.com/luke-h1/lho-lambda/commit/ab613ea867a8fa5435d8a4149a53442181f6dcac))

### Other changes

- **lambda:** remove unused routing ([c929074](https://github.com/luke-h1/lho-lambda/commit/c929074dfd1166e66df0604bafab5b07b3d8fb88))

## 0.3.52 - 2025-02-09

### Other changes

- **lambda:** cleanup pathing ([c1acb5c](https://github.com/luke-h1/lho-lambda/commit/c1acb5ccc2e7845309b2c437ab54c28fcd0203ee))

## 0.3.51 - 2025-01-05

### Other changes

- **lambda:** update to latest node ([fa82ab6](https://github.com/luke-h1/lho-lambda/commit/fa82ab6298988098f219bc26a36c9ef8789488f2))

## 0.3.50 - 2025-01-05

### Other changes

- **lambda:** update deps ([7bb9b11](https://github.com/luke-h1/lho-lambda/commit/7bb9b1186ea57ed2053e877a84ceded693b04993))

## 0.3.49 - 2024-12-27

### Other changes

- **lambda:** update esbuild ([e1cbe24](https://github.com/luke-h1/lho-lambda/commit/e1cbe24e435d6937e418ae99c1beab322cc8c718))

## 0.3.48 - 2024-12-18

### Other changes

- **lambda:** update deps ([0a6c50a](https://github.com/luke-h1/lho-lambda/commit/0a6c50aa032a1ec78d19d875a8a03f5d0a23256c))

## 0.3.47 - 2024-12-10

### Other changes

- **lambda:** update minor deps ([f75825b](https://github.com/luke-h1/lho-lambda/commit/f75825b73d8524fbac17b9d16ef5e5d6df8fb228))

## 0.3.46 - 2024-12-03

### Fixed

- **infrastructure:** pin terraform version due to bug in latest tf version ([22d43d9](https://github.com/luke-h1/lho-lambda/commit/22d43d9c45ec2ac9274a74548bbe67ac087dcb7a))

## 0.3.45 - 2024-12-03

### Fixed

- **infrastructure:** fix terraform plan ([a82c7d5](https://github.com/luke-h1/lho-lambda/commit/a82c7d568dfc79830b316723db32ddcb5ec45d18))

## 0.3.44 - 2024-12-03

### Other changes

- **lambda:** update deps ([19b8ec8](https://github.com/luke-h1/lho-lambda/commit/19b8ec80d3e12c3507790489c18cf24d2d20b972))

## 0.3.43 - 2024-11-16

### Other changes

- **lambda:** update tsconfig deps ([1a42c7e](https://github.com/luke-h1/lho-lambda/commit/1a42c7ed578cbcf6967c54996f0f48406ca60293))

## 0.3.42 - 2024-11-09

### Other changes

- **lambda:** general cleanup ([8d50369](https://github.com/luke-h1/lho-lambda/commit/8d503693ee225e235efcdebc145d463d37ea9ac8))

## 0.3.41 - 2024-11-08

### Fixed

- **infrastructure:** fix cw filters ([40e2fd3](https://github.com/luke-h1/lho-lambda/commit/40e2fd3e55b59f46984d0a7d312b9c3f589058d7))

## 0.3.40 - 2024-11-01

### Other changes

- **infrastructure:** update pnpm ([3642c02](https://github.com/luke-h1/lho-lambda/commit/3642c021820ba04a0723a7be30b853127dd2c8bd))

## 0.3.39 - 2024-11-01

### Other changes

- **lambda:** update esbuild ([de82195](https://github.com/luke-h1/lho-lambda/commit/de8219523e1e4f4ba68ced607e1650393d131298))

## 0.3.38 - 2024-10-31

### Other changes

- **frontend:** update deps ([9a140bc](https://github.com/luke-h1/lho-lambda/commit/9a140bc60862d162b2327e28e6faa28f98221341))

## 0.3.37 - 2024-10-31

### Other changes

- **lambda:** update deps ([9c7a0f5](https://github.com/luke-h1/lho-lambda/commit/9c7a0f5e87a44624d680fbb9c64acdcf86295664))

## 0.3.36 - 2024-10-29

### Other changes

- **tooling:** update deps ([f99dfec](https://github.com/luke-h1/lho-lambda/commit/f99dfec9df2b09213dc8a5db6dae71a1a32e640d))

## 0.3.35 - 2024-10-28

### Fixed

- **lambda:** give lambda cw log settings ([1d89af4](https://github.com/luke-h1/lho-lambda/commit/1d89af4281781af0dd95ebec07e0df62fcfda7f4))

## 0.3.34 - 2024-10-21

### Fixed

- **infrastructure:** prevent branches being out of sync with main ([59a91b1](https://github.com/luke-h1/lho-lambda/commit/59a91b1757e84a747698259a1c1f310d93f3df7a))

## 0.3.33 - 2024-10-21

### Other changes

- **infrastructure:** tweak discord deploy notification ([f448e67](https://github.com/luke-h1/lho-lambda/commit/f448e67c3c863f2e0d9c0fe8591bb03316efb08c))

## 0.3.32 - 2024-10-21

### New

- **infrastructure:** add cloudwatch metric filter ([b04d8f5](https://github.com/luke-h1/lho-lambda/commit/b04d8f5f3bbf86710d538bf623b09a0f04398b92))

## 0.3.31 - 2024-10-20

### Fixed

- **infrastructure:** serverless dynamo for tf locks ([43af045](https://github.com/luke-h1/lho-lambda/commit/43af04585c8d6658489d92b6932931d79179c193))

## 0.3.30 - 2024-10-20

### Other changes

- **documentation:** update readme ([a968b1d](https://github.com/luke-h1/lho-lambda/commit/a968b1db5d1cd0ba560cd35b8b57f4e7afd0a7f3))

## 0.3.29 - 2024-10-20

### Other changes

- **infrastructure:** make comments easier to read ([27f7947](https://github.com/luke-h1/lho-lambda/commit/27f7947fb75bf3192cae23792a8ec0266ff92a90))

## 0.3.28 - 2024-10-20

### Fixed

- **infrastructure:** fix husky warnings ([f2cf755](https://github.com/luke-h1/lho-lambda/commit/f2cf7552c4cb11506dc819e7181690edaa84bc77))

## 0.3.27 - 2024-10-20

### Other changes

- **infrastructure:** update terraform action ([25c6c00](https://github.com/luke-h1/lho-lambda/commit/25c6c00fe75508797c720d71147f0aab611e617c))

## 0.3.26 - 2024-10-20

### Other changes

- **infrastructure:** add project_name variable ([2b6d348](https://github.com/luke-h1/lho-lambda/commit/2b6d3484c5d2d84a2088b9442002b9527f04ddd3))

## 0.3.25 - 2024-10-20

### Other changes

- **lambda:** update deps ([7cf1f3e](https://github.com/luke-h1/lho-lambda/commit/7cf1f3e2d33a9d3a11e38b8a2633b4c73c00d6e4))

## 0.3.24 - 2024-10-13

### Other changes

- **lambda:** logic fix + destroy workflow typo fix ([1ac6163](https://github.com/luke-h1/lho-lambda/commit/1ac61637695a5742ca136ff567fd75e5d421d400))

## 0.3.23 - 2024-10-11

### Other changes

- **lambda:** update eslint ([f398d82](https://github.com/luke-h1/lho-lambda/commit/f398d826b86c9c96992b458340c4a5a8c175f07a))

## 0.3.22 - 2024-10-05

### Other changes

- **lambda:** update deps ([7726805](https://github.com/luke-h1/lho-lambda/commit/7726805389f971e776ac4b1d23c200c8399dd5e3))

## 0.3.21 - 2024-10-05

### Other changes

- **infrastructure:** link to pre-release ([a09895f](https://github.com/luke-h1/lho-lambda/commit/a09895f0cead5943f4eccba5c22bf808c940f2c0))

## 0.3.20 - 2024-10-02

### Other changes

- **infrastructure:** refactor naming conventions ([8bc7490](https://github.com/luke-h1/lho-lambda/commit/8bc74902faac30e6eee84a67446ef648cea06a3e))

## 0.3.19 - 2024-09-22

### Fixed

- **infrastructure:** prevent concurrent terraform deploys ([0bcd9a3](https://github.com/luke-h1/lho-lambda/commit/0bcd9a3e9c270877c300fd5a7b6629858c22bfde))

## 0.3.18 - 2024-09-22

### Other changes

- **ci:** reword notifications ([a9e0bcb](https://github.com/luke-h1/lho-lambda/commit/a9e0bcb79eccd67904a19b51027f7f42f8726457))

## 0.3.17 - 2024-09-22

### Other changes

- **ci:** re-work changelogs to be in their own action ([7f34918](https://github.com/luke-h1/lho-lambda/commit/7f34918d49f73ab0df6ac6d2bb1eddb77f118a40))

## 0.3.16 - 2024-09-22

### Fixed

- **ci:** fix version not updating ([94ad1b1](https://github.com/luke-h1/lho-lambda/commit/94ad1b19e4a262428e62e1cbddb322d377f0d564))

## 0.3.15 - 2024-09-22

### Fixed

- **ci:** fetch before pull ([9205f50](https://github.com/luke-h1/lho-lambda/commit/9205f500f911eeaec78d14e2fce27162b73a430f))

## 0.3.14 - 2024-09-22

### Fixed

- **ci:** dont create changelogs on stg ([6e4a19e](https://github.com/luke-h1/lho-lambda/commit/6e4a19ecc1532eda1f4add86ce76e584331c7a94))

## 0.3.13 - 2024-09-22

### Fixed

- **ci:** fix version on stg ([3b574e2](https://github.com/luke-h1/lho-lambda/commit/3b574e2f5da548e8d979b75e9df8d91e472e52eb))

## 0.3.12 - 2024-09-22

### Fixed

- **ci:** fix latest version not being included in apply ([9fffa17](https://github.com/luke-h1/lho-lambda/commit/9fffa17348e18b13d23b007a805482f1643cdddc))
- **ci:** fix build ([6ef0b75](https://github.com/luke-h1/lho-lambda/commit/6ef0b75840319f50a6ef6435d2d6f0bf118dc7d6))

## 0.3.11 - 2024-09-22

### Fixed

- **ci:** only trigger changelogs via build-release-version ([2de5b76](https://github.com/luke-h1/lho-lambda/commit/2de5b762ff01efdcde548d9aed56534db1de3836))
- **ci:** add git sha to deploy workflow ([7ce7048](https://github.com/luke-h1/lho-lambda/commit/7ce7048670b5343c69e363a22310a896ee6c98f5))
- **ci:** fix changelogs not publishing ([86bb238](https://github.com/luke-h1/lho-lambda/commit/86bb238d73545410ec6ce5e3a58894586af53e3d))

### Other changes

- **ci:** cleanup actions ([a9dc52a](https://github.com/luke-h1/lho-lambda/commit/a9dc52ad4b44d7b24ab029e19a286e11a4ae0d71))
- **ci:** debug event triggers ([96922d1](https://github.com/luke-h1/lho-lambda/commit/96922d1fefe88c5c93cf11eb7646383acf0b8f1f))
- **ci:** debug triggers ([833a344](https://github.com/luke-h1/lho-lambda/commit/833a344fbb853a7122931746ff2be7b78ff23a11))

## 0.3.10 - 2024-09-17

### Fixed

- **ci:** pull latest commits on deploy step ([c2b1154](https://github.com/luke-h1/lho-lambda/commit/c2b11546c6259d537d202953866cf78c5589064b))

## 0.3.9 - 2024-09-17

### Fixed

- **ci:** only run version on live deploys ([365ee86](https://github.com/luke-h1/lho-lambda/commit/365ee8693ae642c5524e4d44d1b0b28b0193ddb3))

## 0.3.8 - 2024-09-17

### Other changes

- **lambda:** update deps ([ebc3bb9](https://github.com/luke-h1/lho-lambda/commit/ebc3bb9633bfe05728545f35373d0d3c5666b410))

## 0.3.7 - 2024-08-30

### Other changes

- **lambda:** add git sha to /version ([a94e7e0](https://github.com/luke-h1/lho-lambda/commit/a94e7e0c085304e512519d80ee28927dfb396045))

## 0.3.6 - 2024-08-14

### Other changes

- **lambda:** refactor ci/cd to use composite actions and update unit tests ([318442b](https://github.com/luke-h1/lho-lambda/commit/318442ba836fa41faeb01e00350d4d8dc269643b))

## 0.3.5 - 2024-08-13

### Other changes

- **infrastructure:** cleanup infra ([85cd233](https://github.com/luke-h1/lho-lambda/commit/85cd2332bcdc7528165ace0b94bdb551ba631901))

## 0.3.4 - 2024-08-11

### Other changes

- **lambda:** update deps ([3019d81](https://github.com/luke-h1/lho-lambda/commit/3019d8138dd002c316f2ee07c5bc0a96e8edb423))

## 0.3.3 - 2024-07-28

### Other changes

- **lambda:** update deps ([90be777](https://github.com/luke-h1/lho-lambda/commit/90be77743108f52d5f0395c8025415c307a86b9a))

## 0.3.2 - 2024-07-07

### Other changes

- **infrastructure:** update setup node version ([7806d77](https://github.com/luke-h1/lho-lambda/commit/7806d7765bbb51f36ad7a4f1df35aae7a4efde0e))

## 0.3.1 - 2024-07-07

### Other changes

- **infrastructure:** update actions/cache version ([fd45f61](https://github.com/luke-h1/lho-lambda/commit/fd45f610c76d8046f4cd7ae6b1db2d2d19f26822))

## 0.3.0 - 2024-07-07

### New

- **lambda:** add node-cache for in-memory caching ([f10909a](https://github.com/luke-h1/lho-lambda/commit/f10909abb5a56c8306b724d1560a99d0f0b12e30))

## 0.2.26 - 2024-07-07

### Other changes

- **lambda:** increase stale time ([46a13e3](https://github.com/luke-h1/lho-lambda/commit/46a13e34babf6c5113cb39b2c6f0b698aed6e924))

## 0.2.25 - 2024-07-07

### Other changes

- **lambda:** update deps ([fae304c](https://github.com/luke-h1/lho-lambda/commit/fae304c0c13cc904286e4f2f9d83b5124aa15f35))

## 0.2.24 - 2024-07-05

### Fixed

- **infrastructure:** fix workflows ([bf93c23](https://github.com/luke-h1/lho-lambda/commit/bf93c233c38a22b6e887eed346eb34554f2902e5))
- **lambda:** fix approval step ([7c089f5](https://github.com/luke-h1/lho-lambda/commit/7c089f53e11d4bb2aa6c7e688835a7ed4896ffbe))
- **infrastructure:** remove approval step due to gh issues ([ccfe8ed](https://github.com/luke-h1/lho-lambda/commit/ccfe8edebfe0fe19c178c7f71ea821e1ea83b5cd))

## 0.2.23 - 2024-06-29

### Other changes

- **lambda:** update PNPM version ([a24af6c](https://github.com/luke-h1/lho-lambda/commit/a24af6c20e68780c953f54737d4a77f6f7a34880))

## 0.2.22 - 2024-06-29

### Other changes

- **lambda:** update alphabetize script ([6f51704](https://github.com/luke-h1/lho-lambda/commit/6f51704b85a7e65996a1e7eedda1cd9e4201592b))
- **lambda:** update alphabetize script ([1d7a128](https://github.com/luke-h1/lho-lambda/commit/1d7a12840f16b045748da4ea7fc820e02c09c3d6))

## 0.2.21 - 2024-06-29

### Other changes

- **lambda:** update deps ([b925093](https://github.com/luke-h1/lho-lambda/commit/b925093db05e6df60489d7113ef315142de4b818))

## 0.2.20 - 2024-06-27

### Other changes

- **lambda:** update deps ([1092a9b](https://github.com/luke-h1/lho-lambda/commit/1092a9b9e3b04b4c8ce1b7ffd7f54251b16b6783))

## 0.2.19 - 2024-06-23

### Other changes

- **lambda:** update deps ([ae4b371](https://github.com/luke-h1/lho-lambda/commit/ae4b371afac8e25bf793c0b1ef2d8fbc6e93776e))

## 0.2.18 - 2024-06-21

### Other changes

- **lambda:** decrease revalidation time ([e022b5f](https://github.com/luke-h1/lho-lambda/commit/e022b5f07e5626b7e4eca1001c8e2e6319356781))

## 0.2.17 - 2024-06-17

### Fixed

- **infrastructure:** fix branch deploys ([8039599](https://github.com/luke-h1/lho-lambda/commit/8039599042803b04d90facb043e0a6bac4c752be))

### Other changes

- **lambda:** update deps ([9ce7040](https://github.com/luke-h1/lho-lambda/commit/9ce7040269062e0f762715f5e7645f0ace973f57))

## 0.2.16 - 2024-06-16

### Other changes

- **lambda:** update deps ([1681f44](https://github.com/luke-h1/lho-lambda/commit/1681f44b5152ccd7154467b9408f941feabeab92))

## 0.2.15 - 2024-06-07

### New

- **infrastructure:** add turborepo to project ([e534b92](https://github.com/luke-h1/lho-lambda/commit/e534b9259913161e0bba62af62952bd5bdb86624))

## 0.2.14 - 2024-06-01

### Other changes

- **lambda:** update dependencies ([701fcb7](https://github.com/luke-h1/lho-lambda/commit/701fcb70fabdf76b209680af7bf86ad40544be17))

## 0.2.13 - 2024-05-19

### Other changes

- **lambda:** update deps ([d3c563e](https://github.com/luke-h1/lho-lambda/commit/d3c563e96443357737aeb8dd2524f955cc83a722))

## 0.2.12 - 2024-05-12

### Other changes

- **lambda:** update deps ([e188c4d](https://github.com/luke-h1/lho-lambda/commit/e188c4d4626e2039c172552a309ef7fb90c44927))

## 0.2.11 - 2024-05-05

### Fixed

- **infrastructure:** fix routing ([ece6d9e](https://github.com/luke-h1/lho-lambda/commit/ece6d9e5712b8521cee16fd8016d16bb61e8f30f))

## 0.2.10 - 2024-05-05

### Fixed

- **lambda:** route ([d6b2a43](https://github.com/luke-h1/lho-lambda/commit/d6b2a431e066645e08661774395b16f40d05b594))

## 0.2.9 - 2024-05-05

### Fixed

- **ci:** fix destroy workflow ([7aa74a3](https://github.com/luke-h1/lho-lambda/commit/7aa74a3dafb52cca0386147df6ef43e13d22c528))
- **lambda:** routes ([ca60747](https://github.com/luke-h1/lho-lambda/commit/ca6074711a7b706c1da5497994f654fdee7e4e05))

## 0.2.8 - 2024-05-04

### Other changes

- **infrastructure:** update pnpm ([28844aa](https://github.com/luke-h1/lho-lambda/commit/28844aa7a2e029a0a007935092ebb0f633b1b4e7))

## 0.2.7 - 2024-05-04

### Other changes

- **lambda:** update deps ([d109bd8](https://github.com/luke-h1/lho-lambda/commit/d109bd880f1567ae5cfd6f0d15c776d52fdc53d8))

## 0.2.6 - 2024-05-02

### Fixed

- **ci:** add missing variable to deploy ci ([a30ae6e](https://github.com/luke-h1/lho-lambda/commit/a30ae6e3987dcfb8b973d1c67b42d28e198befd3))

## 0.2.5 - 2024-05-02

### Other changes

- **infrastructure:** increase burst limit ([9db4c61](https://github.com/luke-h1/lho-lambda/commit/9db4c61c8eb4a4a068676b6c60fa53893c7d8155))

## 0.2.4 - 2024-05-02

### New

- **infrastructure:** increase burst limit ([8bee33e](https://github.com/luke-h1/lho-lambda/commit/8bee33e4b9b00adb137bc1ecb5ab39fefda9ae44))

## 0.2.3 - 2024-05-02

### Other changes

- **lambda:** 2 spaces in json serialization ([c012f79](https://github.com/luke-h1/lho-lambda/commit/c012f79f2caf85f48f0dde52b3e4246c5dc44f4d))

## 0.2.2 - 2024-04-26

### Other changes

- **lambda:** update .gitignore ([7073338](https://github.com/luke-h1/lho-lambda/commit/70733383957cd75a3f0f81464a986964f1c51b0d))

## 0.2.1 - 2024-04-24

### Fixed

- **lambda:** fix version endpoint ([7f843ab](https://github.com/luke-h1/lho-lambda/commit/7f843abe97a6f5fb40d859c5f42e783f523868cb))

## 0.2.0 - 2024-04-24

### New

- **lambda:** add auth lambda ([0e9c91a](https://github.com/luke-h1/lho-lambda/commit/0e9c91ad812ace9df652b21b4bd7b4a10af8013d))

## 0.1.43 - 2024-04-24

### Other changes

- **lambda:** update deps ([3a15197](https://github.com/luke-h1/lho-lambda/commit/3a151970f663e5151ea2c19cd1ed7b217433c796))

## 0.1.42 - 2024-04-20

### Other changes

- **infrastructure:** disable gw logs ([40af759](https://github.com/luke-h1/lho-lambda/commit/40af759f35dc714f8266d06ebaa4f5341b2bd0fd))
- **infrastructure:** disable gw logging ([7ca2145](https://github.com/luke-h1/lho-lambda/commit/7ca21451104f7d976be6c0ae2790cb5951f73839))

## 0.1.41 - 2024-04-20

### Other changes

- **infrastructure:** disable xray ([e880425](https://github.com/luke-h1/lho-lambda/commit/e8804253a3605f3056243992b62b683c11a049f0))

## 0.1.40 - 2024-04-20

### Other changes

- **infrastructure:** disable logging ([b8ae9d3](https://github.com/luke-h1/lho-lambda/commit/b8ae9d3f22d5195b840a11f3a47e61bb57111f89))
- **infrastructure:** disable logging ([840cc33](https://github.com/luke-h1/lho-lambda/commit/840cc330b81d8eb200bef6da23d89023c4a0a706))

## 0.1.39 - 2024-04-19

### Fixed

- **infrastructure:** new certs ([6d1fc84](https://github.com/luke-h1/lho-lambda/commit/6d1fc847ee32c1eac58bd261232572622daea278))
- **infrastructure:** certs ([d61f009](https://github.com/luke-h1/lho-lambda/commit/d61f00966ff7bed076a355723019fd6704ee8eef))

## 0.1.38 - 2024-04-19

### Fixed

- **infrastructure:** destroy workflow ([81d39c0](https://github.com/luke-h1/lho-lambda/commit/81d39c0823713a079cd50e3bf51dbdea1cc8f554))
- **infrastructure:** destroy ([128e588](https://github.com/luke-h1/lho-lambda/commit/128e5887eb8c677eab05e149fdf591e34d6e1ed2))
- **infrastructure:** destroy ([e99ab2f](https://github.com/luke-h1/lho-lambda/commit/e99ab2f1048a06c1cf50ffee802ad2e510350533))

## 0.1.37 - 2024-04-19

### Fixed

- **infrastructure:** log ([40bd754](https://github.com/luke-h1/lho-lambda/commit/40bd754af03c10bc39dc8d86c0d118044d2bab79))

## 0.1.36 - 2024-04-19

### Fixed

- **infrastructure:** log ([2509e53](https://github.com/luke-h1/lho-lambda/commit/2509e53e3d3e30ed2531f3e923e6682d080fb3e3))

## 0.1.35 - 2024-04-19

### Fixed

- **infrastructure:** logging ([b4d10e6](https://github.com/luke-h1/lho-lambda/commit/b4d10e6e1338cf9c6e4275c8db1867ccfc937b53))

## 0.1.34 - 2024-04-19

### Fixed

- **infrastructure:** debug path ([ba252ed](https://github.com/luke-h1/lho-lambda/commit/ba252ed4df76615cf114aa23a16705ab6fc976b5))

## 0.1.33 - 2024-04-19

### Fixed

- **infrastructure:** lambda perms ([dae04e8](https://github.com/luke-h1/lho-lambda/commit/dae04e83e17b03704701af52b1d6ba8b508cf1eb))

## 0.1.32 - 2024-04-18

### Fixed

- **infrastructure:** enable cw ([bfb1102](https://github.com/luke-h1/lho-lambda/commit/bfb1102c238e9c7a864f5343a47a370e1e94ff90))

### Other changes

- **infrastructure:** disable cloudwatch ([4fce693](https://github.com/luke-h1/lho-lambda/commit/4fce693ec8bfd50be864ebe6313bbe86f892dfb5))

## 0.1.30 - 2024-04-17

### Fixed

- **infrastructure:** write to cw ([8bf5bf4](https://github.com/luke-h1/lho-lambda/commit/8bf5bf494968e08ed52bbebdb5377f090bda6396))

## 0.1.29 - 2024-04-17

### Fixed

- **infrastructure:** enable cw logging ([f0f9f8d](https://github.com/luke-h1/lho-lambda/commit/f0f9f8d953cffc5499fa3216340c23021797e5d1))

## 0.1.28 - 2024-04-17

### Fixed

- **infra:** cw remove ([db79900](https://github.com/luke-h1/lho-lambda/commit/db79900ff4140592b20cdb4716ed24613666d26d))

## 0.1.27 - 2024-04-17

### Fixed

- **infrastructure:** disable cw ([3777fbc](https://github.com/luke-h1/lho-lambda/commit/3777fbccca354c28ed0ba18e217aae8ebc3d7515))
- **infrastructure:** cw disable ([4a23f2d](https://github.com/luke-h1/lho-lambda/commit/4a23f2d9f1da3d25252976373f16f68efc311d80))

## 0.1.25 - 2024-04-17

## 0.1.24 - 2024-04-17

### Fixed

- **infrastructure:** deploy ([354e1cb](https://github.com/luke-h1/lho-lambda/commit/354e1cb8df78acdf176a8a37879681d5a6a83a65))

## 0.1.23 - 2024-04-17

### Fixed

- **lambda:** cw ([b27bb5f](https://github.com/luke-h1/lho-lambda/commit/b27bb5fa5c0078897c51bb787f6185b46663168a))

### Other changes

- **lambda:** remove cloudwatch ([1e21186](https://github.com/luke-h1/lho-lambda/commit/1e2118646bd8a621294d0f0325f05a563061841d))
- **lambda:** release ([552fd25](https://github.com/luke-h1/lho-lambda/commit/552fd25e09efeac0b8210639be41c15795e78438))

## 0.1.21 - 2024-04-17

### Other changes

- **infrastructure:** disable cw ([a230b0c](https://github.com/luke-h1/lho-lambda/commit/a230b0cce1df668984f292403a00d6367d215104))

## 0.1.20 - 2024-04-16

### Other changes

- **infrastructure:** deploy test tag ([782b220](https://github.com/luke-h1/lho-lambda/commit/782b2206bfd5d02e9e2584858a68a13f9ea63b3f))

## 0.1.19 - 2024-04-16

### Other changes

- **infrastructure:** change name of log export ([1d0f9bd](https://github.com/luke-h1/lho-lambda/commit/1d0f9bd2e35578b70baa3cab9b705182b8b025b1))

## 0.1.18 - 2024-04-16

### Other changes

- **lambda:** enable log export for lambda cw ([a6dcd49](https://github.com/luke-h1/lho-lambda/commit/a6dcd49acf692687a230cbbdef4d490abcb21a78))

## 0.1.17 - 2024-04-16

### Other changes

- **lambda:** reduce cw retention to 3 days ([f4bcfdf](https://github.com/luke-h1/lho-lambda/commit/f4bcfdfb4d36959ebda56030f814ffa8365a3aaf))

## 0.1.16 - 2024-04-15

### Fixed

- **ci:** add explicit needs block to discord msg ([a93c2ce](https://github.com/luke-h1/lho-lambda/commit/a93c2ce4c2f1a3cd283709dc8f690297b6b18bcf))

## 0.1.15 - 2024-04-14

### Other changes

- **ci:** remove unused step ([4f1c41a](https://github.com/luke-h1/lho-lambda/commit/4f1c41af5a6d34d399e47be7b915a6b004bbb161))

## 0.1.14 - 2024-04-14

### Other changes

- **lambda:** update dev deps ([899ef7c](https://github.com/luke-h1/lho-lambda/commit/899ef7c3de190930fe8d6b4f6104f0191d0c6e43))

## 0.1.13 - 2024-04-14

### Other changes

- **lambda:** reduce cw log retention ([cccaf37](https://github.com/luke-h1/lho-lambda/commit/cccaf37e8dc22c95ff1334121ff0498ac41c42d8))

## 0.1.12 - 2024-04-10

### Fixed

- **infrastructure:** lock tf applies ([5a3ec92](https://github.com/luke-h1/lho-lambda/commit/5a3ec927193252c1fcf628b56b8970f261e41549))
- **infrastructure:** remove tf locks ([ef39daa](https://github.com/luke-h1/lho-lambda/commit/ef39daad76fcf5abad756accb490860d455fb06f))

## 0.1.11 - 2024-04-04

### Fixed

- **lambda:** explict set of cache header if path part is api ([ea7a6fe](https://github.com/luke-h1/lho-lambda/commit/ea7a6fe1f74ae4614460252cdf3aa43519097b41))

## 0.1.10 - 2024-04-01

### Other changes

- **ci:** add manual deploy ci ([48222e3](https://github.com/luke-h1/lho-lambda/commit/48222e3e50f4fabe8b618fadb5c3f8075194ea77))

## 0.1.9 - 2024-04-01

### New

- **ci:** comment deploys ([f589233](https://github.com/luke-h1/lho-lambda/commit/f5892338ca5d48b6975a053f7b7c57a7e6f1cc81))

## 0.1.8 - 2024-03-30

### Other changes

- **ci:** cleanup deploy ci ([aefcdb1](https://github.com/luke-h1/lho-lambda/commit/aefcdb17025edd9691f83578c1d6db4c650aa747))

## 0.1.7 - 2024-03-29

### Other changes

- **ci:** update discord text ([93b1889](https://github.com/luke-h1/lho-lambda/commit/93b1889d8e414ae5b6ecda8ed6a343b4f075d3d1))

## 0.1.6 - 2024-03-29

### New

- **ci:** add stg only deploy ([a9ee841](https://github.com/luke-h1/lho-lambda/commit/a9ee841f0f163486ad31b667e06fcfe833a9ebe3))

## 0.1.5 - 2024-03-29

### New

- **ci:** add staging CI deploys ([8376237](https://github.com/luke-h1/lho-lambda/commit/8376237ba415bc0cc13848a6708c28895d60815d))

## 0.1.4 - 2024-03-29

### Other changes

- **ci:** ensure branch is rebased from main ([9b36ae7](https://github.com/luke-h1/lho-lambda/commit/9b36ae78c35996749339eb1e78dff5755b6258e1))

## 0.1.3 - 2024-03-29

### Other changes

- **lambda:** add metadata to lambda function ([5b4fca6](https://github.com/luke-h1/lho-lambda/commit/5b4fca60b0c12416bb6de4eb14bee8c0f928937d))

## 0.1.2 - 2024-03-29

### Fixed

- **ci:** cat tfplan on approval ([c262319](https://github.com/luke-h1/lho-lambda/commit/c26231988880f6f2a652176dde9cd31d8465853c))
- **ci:** fix work dir ([ada0b5e](https://github.com/luke-h1/lho-lambda/commit/ada0b5e1319b168051af852d4f46ffa301dd54b1))
- **ci:** fix red circle displaying in discord ([d4509b4](https://github.com/luke-h1/lho-lambda/commit/d4509b4b8179b0c8738846bb762dba7a7bc162a1))
- **ci:** test echo body ([211ec29](https://github.com/luke-h1/lho-lambda/commit/211ec29e04b2b10e7046f1586eaff25c45dbfce2))

## 0.1.1 - 2024-03-29

### New

- **ci:** apply on release ([12cf2b8](https://github.com/luke-h1/lho-lambda/commit/12cf2b8d384035e984fe3c12da411f0bdf9ed4f0))
- **ci:** add discord notifications ([accca46](https://github.com/luke-h1/lho-lambda/commit/accca463528b85889eb5cbcfbfaf8d627c6c2a9e))
- **ci:** cat tfplan ([a0754be](https://github.com/luke-h1/lho-lambda/commit/a0754be81431e80ba8703586e4beefa7ab635101))

### Fixed

- **ci:** pull before notifcation ([7c3fa78](https://github.com/luke-h1/lho-lambda/commit/7c3fa783ff3cc41387ab3211b69c3a29d9b81f6e))
- **ci:** pull-requests write ([9cbde5d](https://github.com/luke-h1/lho-lambda/commit/9cbde5db11620959ad53e64287ba709891e596a2))
- **ci:** red check mark ([f01bae5](https://github.com/luke-h1/lho-lambda/commit/f01bae5d10c424b6134146280d27c76d96aa62e4))
- **ci:** github token ([4c4164e](https://github.com/luke-h1/lho-lambda/commit/4c4164ea1ac3e280f59ee01e69e44ae618f3fa9f))

### Other changes

- **lambda:** update discord notifications ([7704e3b](https://github.com/luke-h1/lho-lambda/commit/7704e3b63cb874cb087b8bd49305175886b86e7a))
- **ci:** force version ([d645c66](https://github.com/luke-h1/lho-lambda/commit/d645c666711d8b12fdf35bd75ca91d29af445aaa))

## 0.1.0 - 2024-03-29

### New

- **lambda:** convert to monorepo ([d8f62cd](https://github.com/luke-h1/lho-lambda/commit/d8f62cd587d7972c947ca14e9e91f1b0d3f55ac0))

### Fixed

- **ci:** fix pnpm version ([d4ae827](https://github.com/luke-h1/lho-lambda/commit/d4ae8278faed7225af401a4aff06532d8d36e92f))
- **ci:** fix missing scripts ([1a04482](https://github.com/luke-h1/lho-lambda/commit/1a044829d1dd8d5f6174aabd3011ecc2d0509e4a))
- **ci:** fix aws creds ([e84cc83](https://github.com/luke-h1/lho-lambda/commit/e84cc83bd5a70ae95e9f6dc697643f647082b2b5))
- **ci:** fix working dir terraform ([9894fdb](https://github.com/luke-h1/lho-lambda/commit/9894fdb5315309346151146911187b304243c700))
- **ci:** build lambda before plan ([3cb8b8e](https://github.com/luke-h1/lho-lambda/commit/3cb8b8e21590a7da937a6cb78ea7561335e19ca6))
- **ci:** fix publish step ([5de9450](https://github.com/luke-h1/lho-lambda/commit/5de94508968b4ea64b5c6ddba7c3a08ce04d7126))
- **lambda:** fix deprecated lerna option ([6ac7bed](https://github.com/luke-h1/lho-lambda/commit/6ac7bed3ab9a15bbcfca877cd528387af3313615))
- **lambda:** add publish message ([be6bdaf](https://github.com/luke-h1/lho-lambda/commit/be6bdaf3edacc42089f0783f539d29e7c2c0b594))
- **ci:** add missing commitlint dep ([6a3811b](https://github.com/luke-h1/lho-lambda/commit/6a3811b2699330b0899238832a5542f5518c3071))
- **ci:** lerna version ([313ce7f](https://github.com/luke-h1/lho-lambda/commit/313ce7fb818865988ba2fcf9220dcf8f1d00c21e))
- **ci:** fix lerna version command ([c5bba96](https://github.com/luke-h1/lho-lambda/commit/c5bba965b59b0b8aad17ee8299a653b3666e5280))
- **ci:** no token on publish ([0c0762f](https://github.com/luke-h1/lho-lambda/commit/0c0762f91f9a5b6425d1ef05ae4e4e7a79b8e7a2))
- **ci:** gh token ([a4c0aba](https://github.com/luke-h1/lho-lambda/commit/a4c0abaab911be2861209bb1e58e296b3b7511e3))
- **ci:** gh token ([6b61460](https://github.com/luke-h1/lho-lambda/commit/6b61460da4dd34a613725c3ff043b6da48be8fa9))
- **ci:** contents write ([95ec8e0](https://github.com/luke-h1/lho-lambda/commit/95ec8e0f2d7d713765a6760d9bc4fdf4dd6dbd8f))

### Other changes

- create readme ([c5f01e9](https://github.com/luke-h1/lho-lambda/commit/c5f01e974a62478027b7d59f1f0d8eaad7e15d97))
- create readme ([d1abcc9](https://github.com/luke-h1/lho-lambda/commit/d1abcc96f1b3e2abbdc8c97ecb962536110c7db2))
- create readme ([e38d3df](https://github.com/luke-h1/lho-lambda/commit/e38d3df91b175fd7c7f4399dee7a9168a209c809))
- add rate limits to api ([31e7f94](https://github.com/luke-h1/lho-lambda/commit/31e7f943969620835f12dc6d4236b34f99e6ed65))
- **lambda:** rename lambda ([f8741e7](https://github.com/luke-h1/lho-lambda/commit/f8741e7f402872a6750e9f6ac8bd7f19b19742cf))
- **lambda:** update dependencies ([069d1b0](https://github.com/luke-h1/lho-lambda/commit/069d1b0d7e2603f663a5976361945036e6ff125b))
- **ci:** add release prod ci ([0abe240](https://github.com/luke-h1/lho-lambda/commit/0abe240d287d975bf646da572726550282e243f8))
- **ci:** auth with aws + terraform ([9eed302](https://github.com/luke-h1/lho-lambda/commit/9eed30276c9baa68255048e4d1ef43d831c36af0))
- **ci:** test steps ([4c79ece](https://github.com/luke-h1/lho-lambda/commit/4c79eceb3a71232dce88ac2ca97d0023e4a3e16f))
- **ci:** add secrets ([d118a98](https://github.com/luke-h1/lho-lambda/commit/d118a988f95d6b8582f57dc5de64f5a7084557fb))
