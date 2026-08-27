import { TextField } from "@fluentui/react/lib/TextField";
import { IControl, IControlValue } from "../types";
import React = require("react");
import { splitArray, splitOnCapitalLetters } from "../Utils";
import { ITextFieldStyleProps, ITextFieldStyles } from "@fluentui/react/lib/components/TextField/TextField.types";

export function createInput(control: IControl): any {
  return (
    <>
      <TextField
        key={control.logicalname}
        label={splitOnCapitalLetters(control.label)}
        readOnly
        borderless
        underlined
        value={control.value.toString()}
       />
    </>
  )
}

export function renderTextField(key: string, value: any): any {

  return (
    <>
      <TextField
        label={splitOnCapitalLetters(key)}
        readOnly
        borderless
        underlined
        value={Array.isArray(value) ? splitArray(value.toString()) : value.toString()} />
    </>
  )
}

export function renderTextFieldTest(key: string, value: any): any {

  if (value.toString().includes("#")) {
    return (
      <>
        <TextField
          label={splitOnCapitalLetters(key)}
          readOnly
          borderless
          underlined
          value={value.toString().substring(1)}
          styles={textFieldStyle}
        />
      </>
    )
  }
  else {
    return (
      <>
        <TextField
          label={splitOnCapitalLetters(key)}
          readOnly
          borderless
          underlined
          value={Array.isArray(value) ? splitArray(value.toString()) : value.toString()}
        />
      </>
    )
  }
}

export const textFieldStyle = (props: ITextFieldStyleProps): Partial<ITextFieldStyles> => ({
  ...(props.disabled ? {
      fieldGroup: {
        borderRadius: 0,
        border: "0px solid transparent",
        background: "#F3F2F1"
      },
      field: {
          fontWeight: 600,
          color: "#FF0000",
          backgroundColor: "transparent",
          ":hover": {
              backgroundColor: "rgb(226, 226, 226)"
          }
      }
  } : props.focused ? {
      fieldGroup: {
          border: "none",
          ":after": {
              border: "none"
          }
      },
      field: {
          border: "1px solid rgb(102, 102, 102)",
          color: "#FF0000",
      }
  } : {
      fieldGroup: {
          borderColor: "transparent",
          ":after": {
              border: "none"
          },
          ":hover": {
              borderColor: "rgb(102, 102, 102)",
          }
      },
      field: {
          fontWeight: 600,
          color: "#FF0000",
          ":hover": {
              fontWeight: 400
          }
      }
  })
});


