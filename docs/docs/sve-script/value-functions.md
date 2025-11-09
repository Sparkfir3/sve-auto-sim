---
title: Value Functions
parent: SVE Script
nav_order: 3
---
# Value Functions
Returns a value `n1`. If given `condition` is met, returns `n2` instead.

```
<n1>
OR
<n1><function><n2>
```

Where:
- `<n1>` is the initial value. **Defaults to 0** if blank
- `<function>` is a value function, either conditional or arithmetic (see below)
- `<n2>` is the value to use based on the given conditional function. **Defaults to 1** if blank
	- Conditional - Returns `<n2>` if the conditional function is true
	- Arithmetic - Applies a math function between `<n1>` and `<n2>`
To perform nested value functions, either `n` value can be replaced with another value function inside of brackets `[ ]`

## Dynamic Number Values
The following can be used as `<n1>`  or `<n2>` in place of a constant `int` to return a dynamic value.

| Syntax        | Returns                                                             | Example                                          |
| ------------- | ------------------------------------------------------------------- | ------------------------------------------------ |
| `f(<filter>)` | Count of cards on <u>F</u>ield<br>with filter `<filter>`            | `f(FE)`<br>Number of Evolved Followers on field  |
| `x(<filter>)` | Count of cards in E<u>X</u> area with filter `<filter>`             | `x(KS)`<br>Number of spell tokens in EX area<br> |
| `p(<filter>)` | Count of cards on O<u>p</u>ponent's Field<br>with filter `<filter>` | `p(A)`<br>Number of Amulets on opponent's field  |
| `C`           | <u>C</u>ombo (number of cards played this turn)                     |                                                  |
| `H`           | Number of cards in <u>h</u>and                                      |                                                  |
| `A`           | This card's <u>A</u>ttack                                           |                                                  |
| `D`           | This card's <u>D</u>efense                                          |                                                  |

## Conditional Functions
Conditional functions are case-sensitive. If the conditional function is met, returns `<n2>` instead of `<n1>`. If `<n2>` is not provided and the condition is met, `<n2>` defaults to a value of 1.

| Syntax | Function          | Example                                         |
| ------ | ----------------- | ----------------------------------------------- |
| `c(X)` | <u>C</u>ombo (X)    | `1c(3)4`<br>1, or if Combo (3), then 4.         |
| `s(X)` | <u>S</u>pellchain (X)  | `1s(3)4`<br>1, or if Spellchain (3), then 4.    |
| `o`    | <u>O</u>verflow          | `1o4`<br>1, or if Overflow is active, then 4.   |
| `n(X)` | <u>N</u>ecrocharge (X) | `1n(10)4`<br>1, or if Necrocharge (10), then 4. |

## Arithmetic Functions
Arithmetic functions apply math to both sides of the value formula to calculate the return value. Boolean functions, such as greater than or less than, return 1 if the comparison is true, and return 0 otherwise.

| Syntax | Function     | Example                                                                                 |
| ------ | ------------ | --------------------------------------------------------------------------------------- |
| `+`    | Addition     | `1+H`<br>1 + Number of cards in hand                                                    |
| `-`    | Subtraction  | `C-1`<br>Combo - 1                                                                      |
| `>`    | Greater Than | `A>3`<br>If attack is greater than 3, returns `1`, otherwise `0`                        |
| `<`    | Less Than    | `D<3`<br>If Defense is less than 2, returns `1`, otherwise `0`                          |
| `\|`   | Boolean OR   | `[A>3] \| [D>3]`<br>If Attack or Defense is greater than 3, returns `1`, otherwise `0`  |
| `&`    | Boolean AND  | `[A>3] & [D>3]`<br>If Attack and Defense are greater than 3, returns `1`, otherwise `0` |
