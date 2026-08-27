/* eslint-disable @typescript-eslint/no-explicit-any */
import * as React from 'react';
import { IColumn, Link } from '@fluentui/react';
import { generateLinkToEntityFormFromLookup } from '../../utilities/common/CrmUtils';
import { Context } from '../../types/PcfCoreTypes';

const lookUpCellStyles = {
  root: {
    color: '#0078d4',
    cursor: 'pointer',
    padding: 0,
    whiteSpace: 'normal',
    fontWeight: '400',
    fontSize: 'larger',
  },
};

export const LookUpCell = (
  context: Context,
  item: any,
  column: IColumn
): React.JSX.Element => {
  const [mouseEnter, setMouseEnter] = React.useState<boolean>(false);

  return (
    <Link
      styles={lookUpCellStyles}
      href={generateLinkToEntityFormFromLookup(context, item, column)}
      target='_blank'
      underline={mouseEnter}
      onMouseEnter={() => setMouseEnter(true)}
      onMouseLeave={() => setMouseEnter(false)}
    >
      {item[column.fieldName!]}
    </Link>
  );
};
