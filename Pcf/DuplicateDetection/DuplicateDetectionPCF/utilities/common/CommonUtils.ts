/* eslint-disable @typescript-eslint/no-explicit-any */
import { IColumn } from '@fluentui/react';

export const parseReturnOjectFromSearch = (
  returnObject: string,
  setReturnObject: React.Dispatch<any>
): void => {
  let parsedReturnObject: any = undefined;
  try {
    parsedReturnObject = JSON.parse(returnObject);
  } catch (error) {
    console.log(error);
  } finally {
    setReturnObject(parsedReturnObject);
  }
};

export const parseColumnsFromReturnOject = (
  columnsText: string,
  setColumns: React.Dispatch<React.SetStateAction<any[]>>,
  totalWidth: number
): void => {
  let parsedColumns: IColumn[] = [];
  try {
    parsedColumns = JSON.parse(columnsText);

    if (totalWidth !== -1) {
      const totalWidthViewColumns = parsedColumns.reduce(
        (sum: number, column: any) => {
          return sum + column.minWidth;
        },
        0
      );

      parsedColumns.forEach((column: any) => {
        column.maxWidth =
          column.minWidth +
          (totalWidth - totalWidthViewColumns) / parsedColumns.length;
      });
    }
  } catch (error) {
    console.log(error);
  } finally {
    setColumns(parsedColumns);
  }
};
