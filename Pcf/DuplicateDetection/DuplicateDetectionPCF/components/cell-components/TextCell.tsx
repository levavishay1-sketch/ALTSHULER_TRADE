/* eslint-disable @typescript-eslint/no-explicit-any */
import * as React from 'react';
import {
  IColumn,
  Text,
  TooltipHost,
} from '@fluentui/react';

const lookUpCellStyles = {
  root: {
    padding: 0,
    fontWeight: '400',
  },
};

const calloutProp = { gapSpace: 0 };

const hostStyles = { root: { display: 'block', padding: 0 } };

export const TextCell = (item: any, column: IColumn): React.JSX.Element => {
  return (
    <TooltipHost
      content={item[column.fieldName!]}
      id='TextCellToolTip'
      calloutProps={calloutProp}
      styles={hostStyles}
    >
      <Text
        nowrap={true}
        block={true}
        color='black'
        styles={lookUpCellStyles}
        aria-describedby='TextCellToolTip'
      >
        {item[column.fieldName!]}
      </Text>
    </TooltipHost>
  );
};
