import { DetailsList, IColumn } from "@fluentui/react/lib/DetailsList";
import { splitOnCapitalLetters } from "../Utils";
import { IItem } from "../types";
import React = require("react");
import { Label } from "@fluentui/react/lib/Label";
import { createTheme } from "@fluentui/react/lib/Theme";

export function createDetailsList(items: IItem[], columns: IColumn[], key: string) {
    const labelTheme = createTheme({
      palette: {
        neutralPrimary: '#004578',
      },
    });

    const labelStyles = {
      root: {
        selectors: {
          '.ms-font-l': { color: '#f00'},
        },
      },
    };
    

    return (
      <div>
       <Label styles={labelStyles} theme={labelTheme} >{splitOnCapitalLetters(key)}</Label>
        <DetailsList
          items={items}
          compact={false}
          columns={columns}
          setKey="none"
          isHeaderVisible={true}
        />
      </div>
    );
  }

  export function createRecords(items: any): IItem[] {

    const records: IItem[] = [];
    Object.keys(items).map(key => {
      records.push(
        {
          name: key,
          value: items[key]
        }
      )
    });

    return (
      records
    )
  }
  export function createColumns(items: string[]): IColumn[] {

    const columns: IColumn[] = []; 

    items.map(key => {
      const columnName = splitOnCapitalLetters(key);
      columns.push(
        {
          key: columnName,
          name: columnName,
          minWidth: 70,
          maxWidth: 90,
          isResizable: true,
          data: 'string',
          onRender: (item: IItem) => {
            return <span>{item.value[key]}</span>;
          },
          isPadded: true,
        }
      )
    });

    return (
      columns
    )
  }
