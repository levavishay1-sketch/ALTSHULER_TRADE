import { convertToJson, splitOnCapitalLetters } from "./Utils";
import { IInputs } from "./generated/ManifestTypes";
import { IStackProps, IStackStyles, Stack } from "@fluentui/react/lib/Stack";
import React = require("react");
import * as ReactDom from "react-dom";
import { IControl, IForm, IItem } from "./types";
import { createColumns, createDetailsList, createRecords } from "./formControls/grid";
import { createInput, renderTextField ,renderTextFieldTest} from "./formControls/input";

export class Parser {
  private json: any;
  private test: IForm;

  constructor(
    private container: HTMLDivElement,
    private context: ComponentFramework.Context<IInputs>,
    private submittedFunction: (result: string) => void
  ) {
    this.json = convertToJson<string>(context.parameters.formControls.raw);
    //this.test = convertToJson<IForm>(context.parameters.formControls.raw);
    ReactDom.render(this.parse(), container);
    //ReactDom.render(this.tryParse(), container);
  }

  parse(): JSX.Element {

    const stackTokens = { childrenGap: 10 };
    const stackStyles: Partial<IStackStyles> = { root: { width: 200 } };
    const columnProps: Partial<IStackProps> = {
      tokens: { childrenGap: 10 },
      styles: { root: { width: 200} },
    };
    return (
      <Stack  {...columnProps}>{
        Object.keys(this.json).map(key => {
          const value = this.json[key];

          if (typeof value != 'object') {
            return renderTextFieldTest(key, value)
          } else if (Array.isArray(value)) {

            if (value.every(element => typeof element !== 'object')) {
              return renderTextFieldTest(key, value)

            } else {

              const propertiesNames = Object.keys(this.json[key][0]);
              return createDetailsList(createRecords(this.json[key]), createColumns(propertiesNames), key);
            }
          }
        })
      }</Stack>
    );
  }

  tryParse(): JSX.Element {

    const stackTokens = { childrenGap: 10 };
    const stackStyles: Partial<IStackStyles> = { root: { width: 200 } };
    const columnProps: Partial<IStackProps> = {
      tokens: { childrenGap: 15 },
      styles: { root: { width: 700 } },
    };
    return (
      <Stack  {...columnProps}>
        {
          Object.keys(this.test).map(key => {           

            const controls: IControl[] = this.test[key];
            controls.forEach((control)=>{
              if(control.value != null){
                return createInput(control);
              }
            })
          })
        }
      </Stack>
    );
  }
}