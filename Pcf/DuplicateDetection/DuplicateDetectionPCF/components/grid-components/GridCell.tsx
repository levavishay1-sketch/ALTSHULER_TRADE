/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable react/display-name */
import * as React from 'react';
import { IColumn } from '@fluentui/react';
import { FieldTypes } from '../../enums/FieldTypes';
import { LookUpCell } from '../cell-components/LookUpCell';
import { TextCell } from '../cell-components/TextCell';
import { Context } from '../../types/PcfCoreTypes';

export const GridCell = (
  context: Context
): ((item?: any, index?: number, column?: IColumn) => React.JSX.Element) => {
  return (item?: any, index?: number, column?: IColumn) =>
    item && column ? generateCellByColumnType(context, item, column) : <></>;
};

const generateCellByColumnType = (
  context: Context,
  item: any,
  column: IColumn
): React.JSX.Element => {
  switch ((column as any)['dataType']) {
    case FieldTypes.Lookup: {
      return LookUpCell(context, item, column);
    }
    case FieldTypes.TwoOptions:
    case FieldTypes.DateOnly:
    case FieldTypes.DateAndTime:
    case FieldTypes.Currency:
    case FieldTypes.Decimal:
    case FieldTypes.OptionSet:
    case FieldTypes.WholeNumber:
    case FieldTypes.SingleLineText: {
      return TextCell(item, column);
    }
    default: {
      return TextCell(item, column);
    }
  }
};
