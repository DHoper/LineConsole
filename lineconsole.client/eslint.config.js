import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import simpleImportSort from "eslint-plugin-simple-import-sort";
import importPlugin from "eslint-plugin-import";

export default tseslint.config(
  { ignores: ["dist"] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ["**/*.{ts,tsx}"],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
      "simple-import-sort": simpleImportSort,
      import: importPlugin,
    },
    settings: {
      // 讓 import 規則看得懂 tsconfig 的 @/ 路徑別名
      "import/resolver": { typescript: { project: "./tsconfig.app.json" } },
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      "react-refresh/only-export-components": [
        "warn",
        { allowConstantExport: true },
      ],
      "simple-import-sort/imports": "error",
      "simple-import-sort/exports": "error",
      semi: ["error", "always"],
      indent: ["error", 4],
      // 架構邊界：共用的 components、libs 等不可以反過來 import features（features 依賴共用層，不是反過來）
      "import/no-restricted-paths": [
        "error",
        {
          zones: [
            {
              target: "./src/features",
              from: "./src/app",
            },
            {
              target: [
                "./src/components",
                "./src/hooks",
                "./src/libs",
                "./src/types",
                "./src/store",
              ],
              from: ["./src/features", "./src/app"],
            },
          ],
        },
      ],
    },
  }
);
